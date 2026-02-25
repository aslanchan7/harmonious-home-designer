using System;

namespace Bagua
{
    public enum Element
    {
        Wood,
        Fire,
        Earth,
        Metal,
        Water,
    }

    public enum Color
    {
        Purple,
        Red,
        Pink,
        Green,
        Yellow,
        White,
        Blue,
        Black,
        Gray,
    }

    public enum LifeArea
    {
        WealthAndProsperity,
        FameAndReputation,
        Relationships,
        Family,
        Health,
        ChildrenAndCreativity,
        KnowledgeAndSelfCultivation,
        Career,
        TravelAndHelpfulPeople,
    }

    public static class LifeAreaExtension
    {
        public static Element GetElement(this LifeArea lifeArea)
        {
            switch (lifeArea)
            {
                case LifeArea.WealthAndProsperity:
                case LifeArea.Family:
                    return Element.Wood;
                case LifeArea.FameAndReputation:
                    return Element.Fire;
                case LifeArea.Relationships:
                case LifeArea.Health:
                case LifeArea.KnowledgeAndSelfCultivation:
                    return Element.Earth;
                case LifeArea.ChildrenAndCreativity:
                case LifeArea.TravelAndHelpfulPeople:
                    return Element.Metal;
                default:
                    return Element.Water;
            }
        }
    }

    [Flags]
    public enum Energy
    {
        Chaos = 1,
        Peace = 1 << 1,
        UnaffectedByChaos = 1 << 2,
    }
}
