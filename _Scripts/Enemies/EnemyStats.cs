using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////// AI /////////////////////
// 0: eEnemyAI.CallForBackup;
// 1: eEnemyAI.DontUseMP; TO BE COMPLETED
// 2: eEnemyAI.FightWisely;
// 3: eEnemyAI.FocusOnAttack;
// 4: eEnemyAI.FocusOnDefend;
// 5: eEnemyAI.FocusOnHeal;
// 6: eEnemyAI.Random;
// 7: eEnemyAI.RunAway;

///////////////////// MOVES /////////////////////
//	0: Attack
//	1: Defend
//	2: Run
//	3: Stunned
//	4: Heal Spell
//	5: Attack All
//	6: Call for Backup

[CreateAssetMenu(fileName = "New Enemy Stats")]
public class EnemyStats : ScriptableObject
{
	public int			id;
	public new string	name;
	public Sprite		sprite;
	public int			HP; // set to maxHP in BattleInitiative.cs
	public int			MP; // set to maxMP in BattleInitiative.cs
	public int			maxHP;
	public int			maxMP;
	public int			STR;
	public int			DEF;
	public int			WIS;
	public int			AGI;
	public int			EXP;
	public int			Gold;
	public int			LVL;

	// Drop Item
	public eItem		itemToDrop;
	public float		chanceToDrop;

	// AI
	public eEnemyAI		AI;

	// QuestNdx (If == 0, doesn't progress story) && doesn't StartBattle OnCollision
	public int			questNdx;

	// Default Move
	public float		chanceToCallMove;
	public int			defaultMove;

	// Actions/Moves Enemy can perform
	public List<int>	moveList;

	public int			animNdx;

	public bool			isDead; // set to false in BattleInitiative.cs
}