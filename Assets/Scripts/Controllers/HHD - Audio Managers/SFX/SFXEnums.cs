using System;
using UnityEngine;

public enum SFXAction
{
    Place,
    Pickup,
    Rotate,
    Invalid,
    UI_Click,
    UI_MenuHover1,
    UI_MenuHover2,
    UI_MenuHover3,
    UI_MenuHover4,
    UI_MenuHover5,

    UI_Open,
    UI_Close,
    UI_OpenBook,
    UI_PageForward,
    UI_PageBack,
    UI_CloseBook,
    UI_BoxOpen,
    UI_BoxClose,
    UI_DeleteItem
}

public enum FurnitureSFXCategory
{
    // Default category for furniture without specific SFX sets
    Default,
    // Furniture categories with specific SFX sets
    Chair,
    Wardrobe,
    Lamp,
    Table,
    NightStand,
    Dresser,
    Bed,
    // Miscellaneous categories
    Trash,
    // Plant categories
    Bamboo,
    MoneyTree,
    OrangeBonsai,
    // Lighting categories
    RockLamp,
    PaperLamp


}
