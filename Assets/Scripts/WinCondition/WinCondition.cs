using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bagua;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public static WinCondition Instance;
    public FSBarController BarController;

    public Dictionary<string, List<Furniture>> presentFurnitures = new();
    public PlacedFurnitures.DijkstraCell[,] dijkstraCells;
    public RuleSet ruleSet;

    private Rule.Precondition preconditions = 0;
    private float[] maxPoints = new float[
        Enum.GetNames(typeof(FSEnergyType)).Length
    ];
    public float[] points = new float[
        Enum.GetNames(typeof(FSEnergyType)).Length
    ];
    public float[] lifeAreaPoints = new float[
        Enum.GetNames(typeof(LifeArea)).Length
    ];
    public float[] elementPoints = new float[
        Enum.GetNames(typeof(Element)).Length
    ];
    public int stars = 0;

    public void TryPathfindFrom(DirectedBox door)
    {
        dijkstraCells = PlacedFurnitures.Instance.Dijkstra(
            door.GetNextToRelativeFace(Direction.UP, 1),
            0
        );
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    public void Start()
    {
        foreach (Rule rule in ruleSet.rules)
        {
            RegisterRule(rule);
        }
        float maxFunctionalPoints = maxPoints[(int)FSEnergyType.Functional];
        BarController.SetMax(maxFunctionalPoints);
        points[(int)FSEnergyType.Functional] = maxFunctionalPoints;
        BarController.AddEnergy(
            FSEnergyType.Functional,
            maxFunctionalPoints,
            false
        );
        UpdateRuleCheck();
    }

    public void AddFurniture(Furniture furniture)
    {
        if (!presentFurnitures.ContainsKey(furniture.furnitureName))
        {
            presentFurnitures.Add(
                furniture.furnitureName,
                new List<Furniture>(new Furniture[] { furniture })
            );
        }
        else
        {
            presentFurnitures[furniture.furnitureName].Add(furniture);
        }
    }

    public void RemoveFurniture(Furniture furniture)
    {
        if (presentFurnitures.ContainsKey(furniture.furnitureName))
            presentFurnitures[furniture.furnitureName].Remove(furniture);
    }

    public List<Furniture> GetFurnitures(string furnitures)
    {
        if (!presentFurnitures.ContainsKey(furnitures))
            presentFurnitures.Add(furnitures, new List<Furniture>());
        return presentFurnitures[furnitures];
    }

    public List<BoundingBox> GetZones(string zones)
    {
        return ruleSet.zoneDict[zones];
    }

    public List<DirectedBox> GetFixedItems(string fixedItems)
    {
        return ruleSet.fixedItemDict[fixedItems];
    }

    public void AddMaxPoints(FSEnergyType energyType, float deltaPoints)
    {
        maxPoints[(int)energyType] += deltaPoints;
    }

    public void AddPoints(FSEnergyType energyType, float deltaPoints)
    {
        points[(int)energyType] += deltaPoints;
    }

    public float GetPoints(FSEnergyType energyType)
    {
        return points[(int)energyType];
    }

    public void UpdateRuleCheck()
    {
        float[] oldPoints = (float[])points.Clone();
        float[] oldLifeAreaPoints = (float[])lifeAreaPoints.Clone();
        float[] oldElementPoints = (float[])elementPoints.Clone();
        int oldStars = stars;
        points = new float[oldPoints.Length];
        lifeAreaPoints = new float[oldLifeAreaPoints.Length];
        elementPoints = new float[oldElementPoints.Length];
        stars = 0;
        TryPathfindFrom(GetFixedItems("Main Door")[0]);

        foreach (Rule rule in ruleSet.rules)
        {
            if (rule.precondition == 0)
                rule.Check();
        }

        PreconditionCheck();
        foreach (Rule rule in ruleSet.rules)
        {
            if (preconditions.HasFlag(rule.precondition))
                rule.Check();
        }

        PreconditionFirstStarCheck();
        if (preconditions.HasFlag(Rule.Precondition.FirstStar))
        {
            foreach (Rule rule in ruleSet.rules)
            {
                if (rule.precondition.HasFlag(Rule.Precondition.FirstStar))
                    rule.Check();
            }
        }

        foreach (
            FSEnergyType energyType in Enum.GetValues(typeof(FSEnergyType))
        )
        {
            int i = (int)energyType;
            float deltaPoints = points[i] - oldPoints[i];
            if (deltaPoints < 0)
            {
                BarController.RemoveEnergy(energyType, -deltaPoints, false);
            }
            else if (deltaPoints > 0)
            {
                BarController.AddEnergy(energyType, deltaPoints, false);
            }
        }
        BarController.SetStars(stars);
    }

    private void RegisterRule(Rule rule)
    {
        Rule.RuleArgument[] ruleArguments = rule.ruleFunction.arguments;
        Type[] signature = new Type[ruleArguments.Length];
        object[] ruleBuilderArguments = new object[ruleArguments.Length];
        for (int i = 0; i < ruleArguments.Length; i++)
        {
            Rule.RuleArgument argument = ruleArguments[i];
            switch (argument.type)
            {
                case Rule.ArgumentType.Furniture:
                    signature[i] = typeof(string);
                    ruleBuilderArguments[i] = argument.name;
                    break;
                case Rule.ArgumentType.FixedItem:
                    signature[i] = typeof(List<DirectedBox>);
                    if (!ruleSet.fixedItemDict.ContainsKey(argument.name))
                        throw new Exception(
                            "Fixed item \""
                                + argument.name
                                + "\" is not defined"
                        );
                    ruleBuilderArguments[i] = ruleSet.fixedItemDict[
                        argument.name
                    ];
                    break;
                case Rule.ArgumentType.Zone:
                    signature[i] = typeof(List<BoundingBox>);
                    if (!ruleSet.zoneDict.ContainsKey(argument.name))
                        throw new Exception(
                            "Zone \"" + argument.name + "\" is not defined"
                        );
                    ruleBuilderArguments[i] = ruleSet.zoneDict[argument.name];
                    break;
                case Rule.ArgumentType.Direction:
                    signature[i] = typeof(Direction);
                    ruleBuilderArguments[i] = Enum.Parse<Direction>(
                        argument.name
                    );
                    break;
                case Rule.ArgumentType.Number:
                    signature[i] = typeof(float);
                    ruleBuilderArguments[i] = float.Parse(argument.name);
                    break;
            }
        }
        string methodName = rule.ruleFunction.ruleType.ToString();
        MethodInfo method = typeof(Rule).GetMethod(methodName, signature);
        if (method == null || method.ReturnType != typeof(Action))
        {
            throw new Exception(
                "No rule method Action "
                    + methodName
                    + "("
                    + string.Join(", ", signature.Select(t => t.Name))
                    + ")"
            );
        }
        rule.Check = (Action)method.Invoke(rule, ruleBuilderArguments);
    }

    public void PreconditionCheck()
    {
        preconditions = 0;
        List<string> furnitureList = WinCondition
            .Instance
            .ruleSet
            .furnitureDict["Required"];
        if (
            Functional.Any(
                furnitureList,
                (string furniture) =>
                {
                    List<string> subList = WinCondition
                        .Instance
                        .ruleSet
                        .furnitureDict[furniture];
                    if (subList.Count == 0)
                        return WinCondition
                                .Instance.GetFurnitures(furniture)
                                .Count != 0;
                    else
                        return Functional.Any(
                            subList,
                            furniture =>
                                WinCondition
                                    .Instance.GetFurnitures(furniture)
                                    .Count != 0
                        );
                }
            )
        )
        {
            preconditions |=
                Rule.Precondition.AtLeastOneRequiredFurniturePresent;
        }
        else
        {
            points[(int)FSEnergyType.Functional] = maxPoints[
                (int)FSEnergyType.Functional
            ];
        }
    }

    public void PreconditionFirstStarCheck()
    {
        if (Functional.All(points, point => point == 0))
        {
            stars++;
            preconditions |= Rule.Precondition.FirstStar;
        }
    }
}
