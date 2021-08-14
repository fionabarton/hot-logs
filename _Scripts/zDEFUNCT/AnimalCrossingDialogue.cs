using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Randomly assign dialogue to NPC.
/// Called in RPGEventTrigger attached to NPC gameObject.
/// </summary>

public class AnimalCrossingDialogue : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static AnimalCrossingDialogue _S;
	public static AnimalCrossingDialogue S { get { return _S; } set { _S = value; } }

	// NPC Indexes
	public List<int> 			ndxNPC = new List<int>();

	// Random Index
	public int					randomNdx;

	void Awake() {
		// Singleton
		S = this;
	}

	public void SetDialogue(EventTrigger tEvent){
		// Clear NPC's RPGEventTrigger fields
		tEvent.eventActions.Clear();
		tEvent.option0Actions.Clear();
		tEvent.option1Actions.Clear();

		tEvent.messages0.Clear ();
		tEvent.subMenuMessage0.Clear ();
		tEvent.option0Message.Clear ();
		tEvent.option1Message.Clear ();

		// Assign Events
		//AssignEvents (tEvent, tEvent.npcNdx);
	}

	public void AssignEvents(EventTrigger tEvent, int npcNdx){
		// Random Index
		randomNdx = Random.Range (0, 2);
	
		switch (randomNdx) {
		case 0: // 1) Display Text

			// Assign Events
			tEvent.eventActions.Add (eEventAction.displayText);

			break;
		case 1: // 1) Display Text, 2) Sub Menu
			// Assign Events
			tEvent.eventActions.Add(eEventAction.displayText);
			tEvent.eventActions.Add(eEventAction.subMenu);

			tEvent.option0Actions.Add(eEventAction.displayText);
			tEvent.option1Actions.Add(eEventAction.displayText);

			break;
		}

		// Assign Dialogue
		AssignDialogue(tEvent, npcNdx, randomNdx);
	}


	public void AssignDialogue(EventTrigger tEvent, int npcNdx, int eventsNdx){
		switch (npcNdx) {
		// NPC 1 (Friendly)
		case 1:
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Greeting
			randomNdx = Random.Range (0, 8);
			switch (randomNdx) {
			case 0: tEvent.messages0.Add ("Greetings, " + Stats.S.playerName [0] + "!"); break;
			case 1: tEvent.messages0.Add ("Hello, " + Stats.S.playerName [0] + "!"); break;
			case 2: tEvent.messages0.Add ("Hey, " + Stats.S.playerName [0] + "!"); break;
			case 3: tEvent.messages0.Add ("Hi, " + Stats.S.playerName [0] + "!"); break;
			case 4: tEvent.messages0.Add ("Hiya, " + Stats.S.playerName [0] + "!"); break;
			case 5: tEvent.messages0.Add ("Howdy, " + Stats.S.playerName [0] + "!"); break;
			case 6: tEvent.messages0.Add ("What's up, " + Stats.S.playerName [0] + "!"); break;
			case 7: tEvent.messages0.Add ("Yo, " + Stats.S.playerName [0] + "!"); break;

				/*
				 * GREETINGS*******************************
				 * Ahoy, matey/there
				 * Are you OK?/You alright?/Alright mate?
				 * G'day, mate!
				 * Good day to you!
				 * Good to see you/Nice to see you
				 * Good morning/Good afternoon/Good evening
				 * Greetings
				 * Greetings and salutations!
				 * Hail!
				 * Hello
				 * Hey
				 * Heya
				 * Hi
				 * Hi there
				 * Hiya
				 * Holla
				 * How are things?
				 * How are you doing?
				 * How are you feeling?
				 * How do you do?
				 * How goes it?
				 * How have you been?
				 * How's everything?
				 * How's it going?
				 * How's it hanging?
				 * How's life?
				 * How's your day?
				 * How's your day going?
				 * Howdy
				 * Look what the cat dragged in!
				 * Look who it is!
				 * Oh, yoo hoo!
				 * Peekaboo
				 * Salutations
				 * Sup?/Whazzup?
				 * Top of the morning to you!
				 * What have you been up to?
				 * What's cooking?
				 * What's cracking?
				 * What's going on?
				 * What's happening?
				 * What's kicking?
				 * What's new?
				 * What's popping?
				 * What's shaking?
				 * What's the good word?
				 * What's up?
				 * Why, hello there!
				 * Yo
				 * 
				 * GOODBYES*******************************
				 * Be good
				 * Be well
				 * Bon voyage
				 * Bye
				 * Bye bye
				 * Bye for now
				 * Catch you later
				 * Cheerio
				 * Fare thee well
				 * Farewell
				 * Godspeed
				 * Good day
				 * Good riddance
				 * Goodbye
				 * Goodnight
				 * Gotta boogied
				 * Have a good one
				 * Have a nice day
				 * Have fun
				 * Hope to see you soon
				 * Keep it real
				 * Later (, alligator)
				 * Peace
				 * Peace out
				 * See you around
				 * See you later
				 * See you next time
				 * See you soon
				 * So long
				 * Ta-ta
				 * Take it easy
				 * Toodle-oo
				 * Toodles
				 * 
				 * FOREIGN GREETINGS*******************************
				 * Aloha
				 * Bonjour
				 * Ciao
				 * Hallo
				 * Hola
				 * Konnichiwa
				 * Que Pasa
				 * 
				 * Adieu
				 * Arriverderci
				 * Au revoir
				 * Auf wiedersehen
				 * Ciao
				 * Hasta la vista
				 * Sayonara
				 * 
				 * HOLIDAYS*******************************
				 * Happy holidays 
				 * Season's greetings
				 *
				 * New Year's Day
				 * Valentine's Day
				 * St Patrick's Day
				 * April Fool's Day
				 * Mother's Day
				 * Father's Day
				 * Independence Day
				 * Senior Citizen's Day
				 * Labor Day
				 * Halloween
				 * Thanksgiving Day
				 * Christmas Day
				 * 
				 * INTRODUCTIONS*******************************
				 * Allow me to introduce myself
				 * Glad/Good to know you
				 * Glad/Good to meet you
				 * I am happy to make your acquaintance
				 * It's a pleasure to meet you
				 * It's nice to meet you
				 * Pleased to meet you
				 * 
				 * LONG ABSENCE*******************************
				 * I haven't seen you...
				 * It's been a while
				 * It's been ages
				 * It's been far too long
				 * It's been quite some time
				 * How long ago...
				 * How long has it been?
				 * Long time no see
				 * When was the last time...
				 * 
				 * FRIENDLY*******************************
				 * 
				 * SHY*******************************
				 * 
				 * WEIRD*******************************
				 * 
				 * ANGRY*******************************
				 * What do you want?
				 * What is it this time?
				 * What now?
				 * 
				 * INTROVERTED (Unintentionally ignores Player, inteprets Player's input as his own mind. Eventually realizes it was Player) *******************************
				 * 
				 * NPC: You again? ...can't you just leave me alone...  
				 * Player: 1) You know, I can hear you. 2) Bye!
                 */
			}

			switch (eventsNdx) {
			case 0: // 1) Display Text
				
				randomNdx = Random.Range (0, 9);
				switch (randomNdx) {

				// CAN'T TALK: "Busy"
				case 0:
					tEvent.messages0.Add ("Sorry, but I can't chat right now.");
					tEvent.messages0.Add ("Catch up with me later, okay?");
					break;
				case 1:
					tEvent.messages0.Add ("I'd love to chat, but I'm in a rush. Sorry!");
					break;

				// CAN'T TALK: "Too Much Conversation"
				case 2:
					tEvent.messages0.Add ("I'm all talked out for now.");
					tEvent.messages0.Add ("Gimme some time to recover and we'll talk then!");
					break;
				case 3:
					tEvent.messages0.Add ("Quit bugging me! I love ya, but I've already said it all!");
					break;
				case 4:
					tEvent.messages0.Add ("We've talked enough. I've got nothing left to say!");
					break;

				// CAN'T TALK: "Sick"
				case 5:
					tEvent.messages0.Add ("Sorry, can't talk. My voice is wrecked.");
					break;
				case 6:
					tEvent.messages0.Add ("Sorry, I'm sick. My voice is wrecked. Can't talk. Sorry.");
					break;

				// COMPLIMENT
				case 7:
					tEvent.messages0.Add ("Just felt like you should know...");
					tEvent.messages0.Add ("...you look great today!");
					tEvent.messages0.Add ("Really, I don't know how you do it.");
					break;
				case 8:
					tEvent.messages0.Add ("Dang, I think you're so cool, pal.");
					tEvent.messages0.Add ("Cooler than ice!");
					tEvent.messages0.Add ("Can't get much cooler than that!");
					break;
				}
				break;
			case 1: // 1) Display Text, 2) Sub Menu

				// Wanna talk?
				randomNdx = Random.Range (0, 8);
				switch (randomNdx) {
				case 0: tEvent.subMenuMessage0.Add ("Wanna talk?"); break;
				case 1: tEvent.subMenuMessage0.Add ("C'mon, let's chat!"); break;
				case 2: tEvent.subMenuMessage0.Add ("Listen up, I've got something to say! Wanna listen?"); break;
				case 3: tEvent.subMenuMessage0.Add ("You got time to talk?"); break;
				case 4: tEvent.subMenuMessage0.Add ("Feel like gabbing a bit?"); break;
				case 5: tEvent.subMenuMessage0.Add ("Yap with me! Please?"); break;
				case 6: tEvent.subMenuMessage0.Add ("Got a minute? Lemme chew your ear!"); break;
				case 7: tEvent.subMenuMessage0.Add ("Up for a little chit chat?"); break;
				}


				// NO Message
				randomNdx = Random.Range (0, 6);
				switch (randomNdx) {
				case 0: tEvent.option1Message.Add ("Don't worry about it, later dude."); break;
				case 1: tEvent.option1Message.Add ("Okay, see ya later, pal."); break;
				case 2: tEvent.option1Message.Add ("That's fine, I'm a bit tired anyway."); break;
				case 3: tEvent.option1Message.Add ("No? Okay, that's totally cool."); break;
				case 4: tEvent.option1Message.Add ("No worries, I totally understand!"); break;
				case 5: tEvent.option1Message.Add ("Whoa. That's harsh, man... just kidding. Bye bye!"); break;
				}

				// YES Message
				randomNdx = Random.Range (0, 4);
				switch (randomNdx) {
				case 0:
					tEvent.option0Message.Add ("Great!");
					tEvent.option0Message.Add ("I just read that most of the ants you'll ever see are ladies.");
					tEvent.option0Message.Add ("Crazy, right?!");
					break;
				case 1:
					tEvent.option0Message.Add ("Oh dear, I don't know what to talk about... sorry!");
					break;
				case 2:
					tEvent.option0Message.Add ("Oh...");
					tEvent.option0Message.Add ("...Oops...");
					tEvent.option0Message.Add ("...I actually don't have anything to say.");
					tEvent.option0Message.Add ("I didn't think you were gonna say yes!");
					break;
				case 3:
					tEvent.option0Message.Add ("You're a fortunate person.");
					tEvent.option0Message.Add ("Far too many folks don't take the time to enjoy the little things in life...");
					tEvent.option0Message.Add ("...or they can't afford to for one reason or another.");
					tEvent.option0Message.Add ("Like this silly conversation.");
					tEvent.option0Message.Add ("We're awfully lucky!");
					break;
				}
				break;
			}
			break;
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// NPC 2
		case 2:
			switch (eventsNdx) {
			case 0: // 1) Display Text

				randomNdx = Random.Range (0, 3);

				switch (randomNdx) {
				case 0:
					tEvent.messages0.Add ("Leave me alone!");
					break;
				case 1:
					tEvent.messages0.Add ("Screw off!");
					break;
				case 2:
					tEvent.messages0.Add ("One line of dialogue, hot baby!");
					break;
				}
				break;
			case 1: // 1) Display Text, 2) Sub Menu
				// Random Index
				randomNdx = Random.Range (0, 3);

				switch (randomNdx) {
				case 0:
					tEvent.messages0.Add ("Little man! Yes you!");

					tEvent.subMenuMessage0.Add ("Do I know you?");
					tEvent.option0Message.Add ("Weird, I don't remember you.");
					tEvent.option1Message.Add ("Well, nice to meet you then!");
					break;
				case 1:
					tEvent.messages0.Add ("Ho ho ho!");

					tEvent.subMenuMessage0.Add ("Am I Santa?");
					tEvent.option0Message.Add ("Then eat these toys!");
					tEvent.option1Message.Add ("Damn! What I am going to do with this beard then?!");
					break;
				case 2:
					tEvent.messages0.Add ("Hey yo, baby!");
					tEvent.messages0.Add ("I loooooooovvvvee your stinking face!");

					tEvent.subMenuMessage0.Add ("May I stare at it until death calls upon my mortal frame?");
					tEvent.option0Message.Add ("How kind, how kind!");
					tEvent.option1Message.Add ("Rude you are! Rude, rude, rude!!!");
					break;
				}

				break;
			}
			break;

		// NPC 3
		case 3:
			switch (eventsNdx) {
			case 0: // 1) Display Text

				randomNdx = Random.Range (0, 3);

				switch (randomNdx) {
				case 0:
					tEvent.messages0.Add ("Don't feel like talking. Piss off.");
					break;
				case 1:
					tEvent.messages0.Add ("Little kid, you best get the heck away from me.");
					break;
				case 2:
					tEvent.messages0.Add ("Where am I? Weeeee! Ho! Ho! HO!");
					break;
				}
				break;
			case 1: // 1) Display Text, 2) Sub Menu
				// Random Index
				randomNdx = Random.Range (0, 3);

				switch (randomNdx) {
				case 0:
					tEvent.messages0.Add ("What up.");

					tEvent.subMenuMessage0.Add ("Want me?");
					tEvent.option0Message.Add ("Well I ain't for sale.");
					tEvent.option1Message.Add ("That cool. I don't want me either.");
					break;
				case 1:
					tEvent.messages0.Add ("Hey yo.");

					tEvent.subMenuMessage0.Add ("Wanna talk?");
					tEvent.option0Message.Add ("Well, I got nothin to say. Talk to yourself, ass.");
					tEvent.option1Message.Add ("Good, I didn't want to anyway.");
					break;
				case 2:
					tEvent.messages0.Add ("Rick it up!");

					tEvent.subMenuMessage0.Add ("Beautiful song it is, correct?");
					tEvent.option0Message.Add ("Good taste ye have!");
					tEvent.option1Message.Add ("Terrible! I shall ignore your hate!");
					break;
				}
				break;
			}
			break;
		}
	}


	/*
	GREETING PT 2
	How are you?
	So do ya wanna chat?
	What's going on, babe?
	I'm bored. Talk to me?

	BYE
	Really? Okay, see ya later!
	Are you kidding me?
	That's cool.
	Haha, you're funny! Hasta luego!
	Okay. Next time don't bother me.


	x3: Hey, Yeah, No, Whoa

	Personality: Cranky/ Uchi, Jock/Peppy, Lazy/ Normal, Smug/Snooty

	Conversation Subjects:
	Advice
	Buying/Selling/Trading/Giving Items
	Questions
	Rumors
	Weather

	Favors/Requests:
	Delivery
	Request
	Visit
	Sickness
	Retrieval
	Contest
	Hide & Seek
	New Catchphrase

	Quests:
	Lost Item (ex. Key, Tool)

	NPC Actions:
	Pursue Player
	Shake Trees
	Plant Flowers
	*/
}

