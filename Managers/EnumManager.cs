﻿using System.Collections;
using System.Collections.Generic;

public enum eBattleMode {
    actionButtons, playerTurn, canGoBackToFightButton, canGoBackToFightButtonMultipleTargets,
    qteInitialize, qte, itemOrSpellMenu, triedToRunFromBoss,
    enemyTurn, enemyAction,
    dropItem, addExpAndGold, addExpAndGoldNoDrops, partyDeath, levelUp, returnToWorld, noInputPermitted,
    statusAilment
};

public enum eCamMode { freezeCam, followAll, followUp, followDown, followLR, noTarget };

public enum eDoorMode { open, closed, locked };

public enum eEnemyAI { Random, FocusOnAttack, FocusOnHeal, FocusOnDefend, FightWisely, DontUseMP, RunAway, CallForBackup, Charge };

public enum eEquipScreenMode { pickPartyMember, pickTypeToEquip, noInventory, pickItemToEquip, equippedItem };

public enum eGroundType { desert, dirt, grass, sand, snow };

public enum eItem {
    hpPotion, mpPotion, paperSword, crap, nothing, paperArmor, paperHelmet, paperOther, woodenSword,
    paperWand, berry, smallKey, bug1, bug2, bug3, bug4, bug5, bug6,
    defaultWeapon, defaultArmor, defaultHelmet, defaultAccessory,
    healAllPotion, warpPotion, revivePotion, detoxifyPotion
};
public enum eItemMenuMode { pickItem, pickPartyMember, pickAllPartyMembers, usedItem, pickWhereToWarp };
public enum eItemStatEffect { HP, MP, STR, DEF, WIS, AGI, nothing };
public enum eItemType { Weapon, Armor, Helmet, Accessory, Consumable, Ingredient, Important, Useless };

public enum eMovement { randomWalk, patrol, pursueWalk, pursueRun, pursueWait, pursueDelayedTargetPos, flee, idle, reverse, auto };

public enum eParallax { autoScroll, scrollWithPlayer, childedToPlayer };

public enum ePasswordMode { inactive, inputPassword, checkPassword };

public enum ePlayerMode {
    idle, walkLeft, walkRight, walkUp, walkDown, runLeft, runRight, runUp, runDown,
    walkUpLeft, walkUpRight, walkDownLeft, walkDownRight, runUpLeft, runUpRight, runDownLeft, runDownRight,
    jumpFull, jumpHalf, attack, knockback
};

public enum eQuestAction { deactivateGo, activateGo, changeSprite, changeAnim, changePosition, changeDialogue };

public enum eSaveScreenMode { pickAction, pickFile, subMenu, cannotPeformAction, pickedFile };

public enum eShopkeeperMode { pickBuyOrSell, pickedBuy, pickedSell };

public enum eShopScreenMode { pickItem, itemPurchasedOrSold };

public enum eSongName { nineteenForty, never, ninja, soap, things, startBattle, win, lose, selection, gMinor, zelda };
public enum eSoundName {
    dialogue, selection, damage1, damage2, damage3, death, confirm, deny,
    run, fireball, fireblast, buff1, buff2, highBeep1, highBeep2, swell, flicker
};

public enum eSpellScreenMode {
    pickWhichSpellsToDisplay, pickSpell, doesntKnowSpells,
    pickWhichMemberToHeal, pickAllMembersToHeal, usedSpell, cantUseSpell, pickWhereToWarp
};