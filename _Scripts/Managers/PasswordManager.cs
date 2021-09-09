using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Loops in RPGUpdateManager.cs
/// </summary>

public enum ePasswordMode {inactive, inputPassword, checkPassword};

public class Password {
	public int 				exp1;
	public int 				exp2;
	public int 				gold;
}

public class PasswordManager : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static PasswordManager _S;
	public static PasswordManager S { get { return _S; } set { _S = value; } }

	public Text				passwordText; // Child of RPGCanvas
	public GameObject		passwordGO;

	[Header("Set Dynamically")]
	public string 			passwordString;
	public ePasswordMode 	mode = ePasswordMode.inactive;

	// 3-Letter Words
	private List<string> 		words = new List<string> () {"AAH", "AAL", "AAS", "ABA", "ABB", "ABO", "ABS", "ABY", "ACE", "ACH", "ACT", "ADD", "ADO", "ADS", "ADZ", "AFF", "AFT", "AGA", "AGE", "AGO", "AGS", "AHA", "AHI", "AHS", "AIA", 
		"AID", "AIL", "AIM", "AIN", "AIR", "AIS", "AIT", "AKA", "AKE", "ALA", "ALB", "ALE", "ALF", "ALL", "ALP", "ALS", "ALT", "AMA", "AMI", "AMP", "AMU", "ANA", "AND", "ANE", "ANI", "ANN", "ANT", "ANY", "APE", "APO", "APP", "APT", "ARB", "ARC", "ARD", "ARE", 
		"ARF", "ARK", "ARM", "ARS", "ART", "ARY", "ASH", "ASK", "ASP", "ASS", "ATE", "ATT", "AUA", "AUE", "AUF", "AUK", "AVA", "AVE", "AVO", "AWA", "AWE", "AWL", "AWN", "AXE", "AYE", "AYS", "AYU", "AZO", "BAA", "BAC", "BAD", "BAG", "BAH", "BAL", "BAM", "BAN", 
		"BAP", "BAR", "BAS", "BAT", "BAY", "BED", "BEE", "BEG", "BEL", "BEN", "BES", "BET", "BEY", "BEZ", "BIB", "BID", "BIG", "BIN", "BIO", "BIS", "BIT", "BIZ", "BOA", "BOB", "BOD", "BOG", "BOH", "BOI", "BOK", "BON", "BOO", "BOP", "BOR", "BOS", "BOT", "BOW", 
		"BOX", "BOY", "BRA", "BRO", "BRR", "BRU", "BUB", "BUD", "BUG", "BUM", "BUN", "BUR", "BUS", "BUT", "BUY", "BYE", "BYS", "CAA", "CAB", "CAD", "CAG", "CAM", "CAN", "CAP", "CAR", "CAT", "CAW", "CAY", "CAZ", "CEE", "CEL", "CEP", "CHA", "CHE", "CHI", "CID", 
		"CIG", "CIS", "CIT", "CLY", "COB", "COD", "COG", "COL", "CON", "COO", "COP", "COR", "COS", "COT", "COW", "COX", "COY", "COZ", "CRU", "CRY", "CUB", "CUD", "CUE", "CUM", "CUP", "CUR", "CUT", "CUZ", "CWM", "DAB", "DAD", "DAE", "DAG", "DAH", "DAK", "DAL", 
		"DAM", "DAN", "DAP", "DAS", "DAW", "DAY", "DEB", "DEE", "DEF", "DEG", "DEI", "DEL", "DEN", "DEV", "DEW", "DEX", "DEY", "DIB", "DID", "DIE", "DIF", "DIG", "DIM", "DIN", "DIP", "DIS", "DIT", "DIV", "DOB", "DOC", "DOD", "DOE", "DOF", "DOG", "DOH", "DOL", 
		"DOM", "DON", "DOO", "DOP", "DOR", "DOS", "DOT", "DOW", "DOY", "DRY", "DSO", "DUB", "DUD", "DUE", "DUG", "DUH", "DUI", "DUN", "DUO", "DUP", "DUX", "DYE", "DZO", "EAN", "EAR", "EAS", "EAT", "EAU", "EBB", "ECH", "ECO", "ECU", "EDH", "EDS", "EEK", "EEL", 
		"EEN", "EFF", "EFS", "EFT", "EGG", "EGO", "EHS", "EIK", "EKE", "ELD", "ELF", "ELK", "ELL", "ELM", "ELS", "ELT", "EME", "EMO", "EMS", "EMU", "END", "ENE", "ENG", "ENS", "EON", "ERA", "ERE", "ERF", "ERG", "ERK", "ERN", "ERR", "ERS", "ESS", "EST", "ETA", 
		"ETH", "EUK", "EVE", "EVO", "EWE", "EWK", "EWT", "EXO", "EYE", "FAA", "FAB", "FAD", "FAE", "FAG", "FAH", "FAN", "FAP", "FAR", "FAS", "FAT", "FAW", "FAX", "FAY", "FED", "FEE", "FEG", "FEH", "FEM", "FEN", "FER", "FES", "FET", "FEU", "FEW", "FEY", "FEZ", 
		"FIB", "FID", "FIE", "FIG", "FIL", "FIN", "FIR", "FIT", "FIX", "FIZ", "FLU", "FLY", "FOB", "FOE", "FOG", "FOH", "FON", "FOP", "FOR", "FOU", "FOX", "FOY", "FRA", "FRO", "FRY", "FUB", "FUD", "FUG", "FUM", "FUN", "FUR", "GAB", "GAD", "GAE", "GAG", "GAL", 
		"GAM", "GAN", "GAP", "GAR", "GAS", "GAT", "GAU", "GAY", "GED", "GEE", "GEL", "GEM", "GEN", "GEO", "GET", "GEY", "GHI", "GIB", "GID", "GIE", "GIF", "GIG", "GIN", "GIO", "GIP", "GIS", "GIT", "GJU", "GNU", "GOA", "GOB", "GOD", "GOE", "GON", "GOO", "GOR", 
		"GOS", "GOT", "GOV", "GOX", "GOY", "GUB", "GUE", "GUL", "GUM", "GUN", "GUP", "GUR", "GUS", "GUT", "GUV", "GUY", "GYM", "GYP", "HAD", "HAE", "HAG", "HAH", "HAJ", "HAM", "HAN", "HAO", "HAP", "HAS", "HAT", "HAW", "HAY", "HEH", "HEM", "HEN", "HEP", "HER", 
		"HES", "HET", "HEW", "HEX", "HEY", "HIC", "HID", "HIE", "HIM", "HIN", "HIP", "HIS", "HIT", "HMM", "HOA", "HOB", "HOC", "HOD", "HOE", "HOG", "HOH", "HOI", "HOM", "HON", "HOO", "HOP", "HOS", "HOT", "HOW", "HOX", "HOY", "HUB", "HUE", "HUG", "HUH", "HUI", 
		"HUM", "HUN", "HUP", "HUT", "HYE", "HYP", "ICE", "ICH", "ICK", "ICY", "IDE", "IDS", "IFF", "IFS", "IGG", "ILK", "ILL", "IMP", "INK", "INN", "INS", "ION", "IOS", "IRE", "IRK", "ISH", "ISM", "ISO", "ITA", "ITS", "IVY", "IWI", "JAB", "JAG", "JAI", "JAK", 
		"JAM", "JAP", "JAR", "JAW", "JAY", "JEE", "JET", "JEU", "JEW", "JIB", "JIG", "JIN", "JIZ", "JOB", "JOE", "JOG", "JOL", "JOR", "JOT", "JOW", "JOY", "JUD", "JUG", "JUN", "JUS", "JUT", "KAB", "KAE", "KAF", "KAI", "KAK", "KAM", "KAS", "KAT", "KAW", "KAY", 
		"KEA", "KEB", "KED", "KEF", "KEG", "KEN", "KEP", "KET", "KEX", "KEY", "KHI", "KID", "KIF", "KIN", "KIP", "KIR", "KIS", "KIT", "KOA", "KOB", "KOI", "KON", "KOP", "KOR", "KOS", "KOW", "KUE", "KYE", "KYU", "LAB", "LAC", "LAD", "LAG", "LAH", "LAM", "LAP", 
		"LAR", "LAS", "LAT", "LAV", "LAW", "LAX", "LAY", "LEA", "LED", "LEE", "LEG", "LEI", "LEK", "LEP", "LES", "LET", "LEU", "LEV", "LEW", "LEX", "LEY", "LEZ", "LIB", "LID", "LIE", "LIG", "LIN", "LIP", "LIS", "LIT", "LOB", "LOD", "LOG", "LOO", "LOP", "LOR", 
		"LOS", "LOT", "LOU", "LOW", "LOX", "LOY", "LUD", "LUG", "LUM", "LUR", "LUV", "LUX", "LUZ", "LYE", "LYM", "MAA", "MAC", "MAD", "MAE", "MAG", "MAK", "MAL", "MAM", "MAN", "MAP", "MAR", "MAS", "MAT", "MAW", "MAX", "MAY", "MED", "MEE", "MEG", "MEL", "MEM", 
		"MEN", "MES", "MET", "MEU", "MEW", "MHO", "MIB", "MIC", "MID", "MIG", "MIL", "MIM", "MIR", "MIS", "MIX", "MIZ", "MNA", "MOA", "MOB", "MOC", "MOD", "MOE", "MOG", "MOI", "MOL", "MOM", "MON", "MOO", "MOP", "MOR", "MOS", "MOT", "MOU", "MOW", "MOY", "MOZ", 
		"MUD", "MUG", "MUM", "MUN", "MUS", "MUT", "MUX", "MYC", "NAB", "NAE", "NAG", "NAH", "NAM", "NAN", "NAP", "NAS", "NAT", "NAW", "NAY", "NEB", "NED", "NEE", "NEF", "NEG", "NEK", "NEP", "NET", "NEW", "NIB", "NID", "NIE", "NIL", "NIM", "NIP", "NIS", "NIT", 
		"NIX", "NOB", "NOD", "NOG", "NOH", "NOM", "NON", "NOO", "NOR", "NOS", "NOT", "NOW", "NOX", "NOY", "NTH", "NUB", "NUN", "NUR", "NUS", "NUT", "NYE", "NYS", "OAF", "OAK", "OAR", "OAT", "OBA", "OBE", "OBI", "OBO", "OBS", "OCA", "OCH", "ODA", "ODD", "ODE", 
		"ODS", "OES", "OFF", "OFT", "OHM", "OHO", "OHS", "OIK", "OIL", "OKA", "OKE", "OLD", "OLE", "OLM", "OMS", "ONE", "ONO", "ONS", "ONY", "OOF", "OOH", "OOM", "OON", "OOP", "OOR", "OOS", "OOT", "OPE", "OPS", "OPT", "ORA", "ORB", "ORC", "ORD", "ORE", "ORF",
		"ORS", "ORT", "OSE", "OUD", "OUK", "OUP", "OUR", "OUS", "OUT", "OVA", "OWE", "OWL", "OWN", "OWT", "OXO", "OXY", "OYE", "OYS", "PAC", "PAD", "PAH", "PAL", "PAM", "PAN", "PAP", "PAR", "PAS", "PAT", "PAV", "PAW", "PAX", "PAY", "PEA", "PEC", "PED", "PEE", 
		"PEG", "PEH", "PEN", "PEP", "PER", "PES", "PET", "PEW", "PHI", "PHO", "PHT", "PIA", "PIC", "PIE", "PIG", "PIN", "PIP", "PIR", "PIS", "PIT", "PIU", "PIX", "PLU", "PLY", "POA", "POD", "POH", "POI", "POL", "POM", "POO", "POP", "POS", "POT", "POW", "POX", 
		"POZ", "PRE", "PRO", "PRY", "PSI", "PST", "PUB", "PUD", "PUG", "PUH", "PUL", "PUN", "PUP", "PUR", "PUS", "PUT", "PUY", "PYA", "PYE", "PYX", "QAT", "QIS", "QUA", "RAD", "RAG", "RAH", "RAI", "RAJ", "RAM", "RAN", "RAP", "RAS", "RAT", "RAW", "RAX", "RAY", 
		"REB", "REC", "RED", "REE", "REF", "REG", "REH", "REI", "REM", "REN", "REO", "REP", "RES", "RET", "REV", "REW", "REX", "REZ", "RHO", "RHY", "RIA", "RIB", "RID", "RIF", "RIG", "RIM", "RIN", "RIP", "RIT", "RIZ", "ROB", "ROC", "ROD", "ROE", "ROK", "ROM", 
		"ROO", "ROT", "ROW", "RUB", "RUC", "RUD", "RUE", "RUG", "RUM", "RUN", "RUT", "RYA", "RYE", "SAB", "SAC", "SAD", "SAE", "SAG", "SAI", "SAL", "SAM", "SAN", "SAP", "SAR", "SAT", "SAU", "SAV", "SAW", "SAX", "SAY", "SAZ", "SEA", "SEC", "SED", "SEE", "SEG", 
		"SEI", "SEL", "SEN", "SER", "SET", "SEW", "SEX", "SEY", "SEZ", "SHA", "SHE", "SHH", "SHY", "SIB", "SIC", "SIF", "SIK", "SIM", "SIN", "SIP", "SIR", "SIS", "SIT", "SIX", "SKA", "SKI", "SKY", "SLY", "SMA", "SNY", "SOB", "SOC", "SOD", "SOG", "SOH", "SOL", 
		"SOM", "SON", "SOP", "SOS", "SOT", "SOU", "SOV", "SOW", "SOX", "SOY", "SPA", "SPY", "SRI", "STY", "SUB", "SUD", "SUE", "SUI", "SUK", "SUM", "SUN", "SUP", "SUQ", "SUR", "SUS", "SWY", "SYE", "SYN", "TAB", "TAD", "TAE", "TAG", "TAI", "TAJ", "TAK", "TAM", 
		"TAN", "TAO", "TAP", "TAR", "TAS", "TAT", "TAU", "TAV", "TAW", "TAX", "TAY", "TEA", "TEC", "TED", "TEE", "TEF", "TEG", "TEL", "TEN", "TES", "TET", "TEW", "TEX", "THE", "THO", "THY", "TIC", "TID", "TIE", "TIG", "TIL", "TIN", "TIP", "TIS", "TIT", "TIX", 
		"TOC", "TOD", "TOE", "TOG", "TOM", "TON", "TOO", "TOP", "TOR", "TOT", "TOW", "TOY", "TRY", "TSK", "TUB", "TUG", "TUI", "TUM", "TUN", "TUP", "TUT", "TUX", "TWA", "TWO", "TWP", "TYE", "TYG", "UDO", "UDS", "UEY", "UFO", "UGH", "UGS", "UKE", "ULE", "ULU", 
		"UMM", "UMP", "UMU", "UNI", "UNS", "UPO", "UPS", "URB", "URD", "URE", "URN", "URP", "USE", "UTA", "UTE", "UTS", "UTU", "UVA", "VAC", "VAE", "VAG", "VAN", "VAR", "VAS", "VAT", "VAU", "VAV", "VAW", "VEE", "VEG", "VET", "VEX", "VIA", "VID", "VIE", "VIG", 
		"VIM", "VIN", "VIS", "VLY", "VOE", "VOL", "VOR", "VOW", "VOX", "VUG", "VUM", "WAB", "WAD", "WAE", "WAG", "WAI", "WAN", "WAP", "WAR", "WAS", "WAT", "WAW", "WAX", "WAY", "WEB", "WED", "WEE", "WEM", "WEN", "WET", "WEX", "WEY", "WHA", "WHO", "WHY", "WIG", 
		"WIN", "WIS", "WIT", "WIZ", "WOE", "WOF", "WOG", "WOK", "WON", "WOO", "WOP", "WOS", "WOT", "WOW", "WOX", "WRY", "WUD", "WUS", "WYE", "WYN", "XIS", "YAD", "YAE", "YAG", "YAH", "YAK", "YAM", "YAP", "YAR", "YAW", "YAY", "YEA", "YEH", "YEN", "YEP", "YES", 
		"YET", "YEW", "YEX", "YGO", "YID", "YIN", "YIP", "YOB", "YOD", "YOK", "YOM", "YON", "YOS", "YOU", "YOW", "YUG", "YUK", "YUM", "YUP", "YUS", "ZAG", "ZAP", "ZAS", "ZAX", "ZEA", "ZED", "ZEE", "ZEK", "ZEL", "ZEP", "ZEX", "ZHO", "ZIG", "ZIN", "ZIP", "ZIT", 
		"ZIZ", "ZOA", "ZOL", "ZOO", "ZOS", "ZUZ", "ZZZ"};

	// Passwords of two combined 3-letter words
	private List<string> 	passwords = new List<string>();

	void Awake() {
		// Singleton
		S = this;
	}

	void Start (){
		passwordGO.SetActive (false);

		//Debug.Log ("Amount of 3 Letter Words: " + words.Count);

		SpawnPasswords ();

		// Add Loop() to UpdateManager
		UpdateManager.updateDelegate += Loop;
	}

	public void Loop () {
		switch (mode) {
		case ePasswordMode.inactive:
			// Activate Password Mode
			if (Input.GetKeyDown (KeyCode.Space)) {
				// Reset Password String
				passwordString = "";

				mode = ePasswordMode.inputPassword;

				passwordGO.SetActive (true);
				passwordText.text = "Password: " + passwordString;

				// Pause World
				RPG.S.paused = true;
			}
			break;
		case ePasswordMode.inputPassword:
			// Deactivate Password Mode
			if (Input.GetKeyDown (KeyCode.Space)) {
				DeactivatePassword();
			}

			// Password Input
			if (passwordGO.activeInHierarchy) {
				foreach (char c in Input.inputString) {
					if (c == '\b') { // has backspace/delete been pressed?

						if (passwordString.Length != 0) {
							passwordString = passwordString.Substring (0, passwordString.Length - 1);

							passwordText.text = "Password: " + passwordString;
						}
					} else if ((c == '\n') || (c == '\r')) { // enter/return
						mode = ePasswordMode.checkPassword;

						StartCoroutine("CheckPassword", passwordString);
					} else {
						passwordString += c;

						passwordText.text = "Password: " + passwordString;
					}
				}
			}
			break;
		case ePasswordMode.checkPassword:
			// Deactivate Password Mode
			if (Input.GetKeyDown (KeyCode.Space)) {
				DeactivatePassword();
			}
			break;
		}
	}
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Example Passwords: 05051000 = Player 1: Level 5, Player 2: Level 5, 1000 Gold
    /// </summary>
    /// <param name="tString"></param>
    /// <returns></returns>
    IEnumerator CheckPassword(string tString){

		if (tString.Length >= 5) {
			// Seperate password
			string lvl1 = tString.Substring (0, 2);
			string lvl2 = tString.Substring (2, 2);
			string gold = tString.Substring (4);

			// Convert strings into ints
			int tLvl1 = System.Convert.ToInt32 (lvl1);
			int tLvl2 = System.Convert.ToInt32 (lvl2);

			// Ensures Levels are between 1 and 10
			if ((tLvl1 >= 1 && tLvl1 <= 10) && (tLvl2 >= 1 && tLvl2 <= 10)) {
				// Set Player 1 Lvl
				PartyStats.S.LVL[0] = tLvl1;

				// Set Player 1 Exp
				switch (tLvl1) {
				case 2:
					PartyStats.S.EXP[0] = 8;
					break;
				case 3:
					PartyStats.S.EXP[0] = 24;
					break;
				case 4:
					PartyStats.S.EXP[0] = 48;
					break;
				case 5:
					PartyStats.S.EXP[0] = 111;
					break;
				case 6:
					PartyStats.S.EXP[0] = 221;
					break;
				case 7:
					PartyStats.S.EXP[0] = 451;
					break;
				case 8:
					PartyStats.S.EXP[0] = 801;
					break;
				case 9:
					PartyStats.S.EXP[0] = 1300;
					break;
				case 10:
					PartyStats.S.EXP [0] = 2001;
					break;
				default:
					break;
				}

				// Set Player 2 Lvl
				PartyStats.S.LVL[1] = tLvl2;

				// Set Player 2 Exp
				switch (tLvl2) {
				case 2:
					PartyStats.S.EXP [1] = 10;
					break;
				case 3:
					PartyStats.S.EXP[1] = 24;
					break;
				case 4:
					PartyStats.S.EXP[1] = 56;
					break;
				case 5:
					PartyStats.S.EXP[1] = 111;
					break;
				case 6:
					PartyStats.S.EXP[1] = 251;
					break;
				case 7:
					PartyStats.S.EXP[1] = 451;
					break;
				case 8:
					PartyStats.S.EXP[1] = 851;
					break;
				case 9:
					PartyStats.S.EXP[1] = 1301;
					break;
				case 10:
					PartyStats.S.EXP[1] = 2101;
					break;
				default:
					break;
				}

				CheckLevelUp ();

				// Gold
				int tInt3 = System.Convert.ToInt32 (gold);
				PartyStats.S.Gold = tInt3 * 100;

			} else {
				passwordText.text = "Password: INVALID!";
			}

		} else {
			passwordText.text = "Password: INVALID!";
		}

		// Wait 2 Seconds
		yield return new WaitForSeconds (1.5f);

		// Deactivate Password Mode
		DeactivatePassword();
	}
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	void DeactivatePassword(){
		mode = ePasswordMode.inactive;
		passwordGO.SetActive (false);
		RPG.S.paused = false;
	}

	void CheckLevelUp(){
		// Level Up
		PartyStats.S.CheckForLevelUp ();
		PartyStats.S.hasLevelledUp[0] = false;
		PartyStats.S.hasLevelledUp[1] = false;

		passwordText.text = "Password: ACCEPTED!";
	}

	void SpawnPasswords(){
		// Rearrange First-3 and Last-3
		List<string> first3 = words.OrderByDescending(str => str[0]).ToList();
		List<string> last3 = words.OrderBy(str => str[2]).ToList();

		// Combine First-3 and Last-3 into Passwords
		for (int i = 0; i < first3.Count; i++) {
			for (int j = 0; j < last3.Count; j++) {
				passwords.Add (first3[i] + last3[j]);
			}
		}
	}
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////
}