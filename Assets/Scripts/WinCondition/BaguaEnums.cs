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

    [Flags]
    public enum Energy
    {
        Chaos = 1,
        Peace = 1 << 1,
    }
}
