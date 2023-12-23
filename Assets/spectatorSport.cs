using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class spectatorSport : MonoBehaviour {
	
    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio audio;
	public KMBossModule boss;
	
	private bool isSolved;
	private static int moduleCount = 1;
    private int moduleId;
	
	public GameObject feed_item;
	public GameObject feed_list;
	public GameObject question_item;
	
	public UnityEngine.UI.Text question_text;
	
	public Sprite[] icons;
	public Sprite[] obj_icons;
	
	public KMSelectable[] answer_list;
	public KMSelectable solveb;
	
	public static string[] ignoredModules = null;
	
	private string type = "Arena";
	private string correct;
	
	private int solves = 0;
	private int stage = 0;
	private int lastStage;
	
	private string state = "start";
	
	private string[] questions = new string[] {"Which player had the most kills in the round?", "Which player perished the most this round?", "Which team has gained the victory in this round?", "Which gamemode was everyone playing?", "Which weapon was the most employed?", "How many critical hits were in this round?", "playerkills", "playerdeaths", "playerkills", "playerdeaths", "Which player managed to get the least amount of kills in the round?", "weaponkills", "weaponkills", "trackerkill"};

	private string[] weapon_list = new string[] { "Backstab","Bat","Black Box","Bonesaw","Bottle","Chargin Targe","Fire Axe","Fists","Flame Thrower","Flare Gun","Grand Slam","Grenade Launcher","Gunslinger","Knife","Kukri","Minigun","Revolver","Rocket Launcher","Scattergun","Sentry","Shovel","SMG","Sniper Rifle","Sniper Rifle(HS)","Syringe Gun","Widowmaker","Winger","Wrench","Cow Mangler 5000","Golden Frying Pan" };
	private string[] player_list = new string[] { "ThatOneKid", "xXxSlay3rXxX", "Bob", "Rick", "OhNoesWoes", "Mickeroni", "Tyler-Y", "Resent", "FluffyRuffian", "FitDwarf", "Gnomeo", "FRQuilo", "Dr_ex", "CrazyTalk", "KrazyTalk", "CrazyChalk", "Cypher", "AsmallBaby", "||||||||", "Noodle Legs", "MiB", "Mashomatics", "Morseomatics", "Mr.Punchy", "Cyanix", "FRKilo", "MrX", "Forget Maze", "Alpha4UpDown", "BunchOfLines", "AllUPPERCASE", "allupercase", "XxXSlayerxXx", "lowercase", "Diamonds", "Blank", "Nothing", "Abort", "Press", "Hold", "Cyan", "Magenta", "FastMath", "GhostSalt", "QuickMath", "GhostCoast", "Staircase", "Its empty", "Jeff", "Geoff" };
	private string[] team_list = new string[] { "RED", "BLU", "Event" };
	private int[] team_scores = new int[2];
	private Dictionary<string, int> kill_scores = new Dictionary<string, int>();
	private Dictionary<string, int> death_scores = new Dictionary<string, int>();
	private Dictionary<string, int> weapon_scores = new Dictionary<string, int>();
	
	private Dictionary<string, Dictionary<string, int>> domination_count = new Dictionary<string, Dictionary<string, int>>();
	private Dictionary<string, Dictionary<string, int>> kill_tracker = new Dictionary<string, Dictionary<string, int>>();
	
	private string[] BLUTeam = new string[7];
	private string[] REDTeam = new string[7];
	
	private int crit_counter = 0;
	
	void Start () {
		if (ignoredModules == null) {
			ignoredModules = boss.GetIgnoredModules("Spectator Sport", new string[]{
				"14",
				"42",
				"501",
				"Forget Enigma",
				"Forget Everything",
				"Forget It Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Colors",
				"Forget Them All",
				"Temporal Sequence",
				"Forget This",
				"Forget Us Not",
				"OmegaForget",
				"Organization",
				"Power Button",
				"Purgatory",
				"Simon Forgets",
				"Simon's Stages",
				"Souvenir",
				"Whiteout",
				"Übermodule",
				"Spectator Sport",
			});
		};
		
		lastStage = bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).ToList().Count;
		Debug.LogFormat("[Spectator Sport #{0}] Total stages: {1}.", moduleId, lastStage.ToString());
		
		MakeTeams();
		
		question_item.SetActive(false);
		
		if (lastStage < 2) { 
			Debug.LogFormat("[Spectator Sport #{0}] Not enough not ignored modules, solving.", moduleId);
			Solve(); 
			return; 
		}
		
		foreach (string gun in weapon_list) {
			weapon_scores.Add(gun, 0);
		}
		
	}
	
	
	void MakeTeams() {
		for (int i = 0; i < 7; i++) {
			string name = player_list[UnityEngine.Random.Range(0,player_list.Length)];
			while (BLUTeam.Contains(name)) {
				name = player_list[UnityEngine.Random.Range(0,player_list.Length)];
			}
			BLUTeam[i] = name;
			kill_scores.Add(name, 0);
			death_scores.Add(name, 0);
		}
		
		for (int i = 0; i < 7; i++) {
			string name = player_list[UnityEngine.Random.Range(0,player_list.Length)];
			while (BLUTeam.Contains(name) || REDTeam.Contains(name)) {
				name = player_list[UnityEngine.Random.Range(0,player_list.Length)];
			}
			REDTeam[i] = name;
			kill_scores.Add(name, 0);
			death_scores.Add(name, 0);
		}
		
		string[] allplayers = REDTeam.Concat(BLUTeam).ToArray();
		
		foreach (string player in allplayers) {
			domination_count.Add(player, new Dictionary<string, int>());
			kill_tracker.Add(player, new Dictionary<string, int>());
			foreach (string e in allplayers) {
				if ( e == player ) continue;
				domination_count[player].Add(e, 0);
				kill_tracker[player].Add(e, 0);
			}
		}			
	}
	
	void AddLine () {
		int rnd1 = UnityEngine.Random.Range(0,BLUTeam.Length);
		int rnd2 = UnityEngine.Random.Range(0,REDTeam.Length);
		
		int rndw = UnityEngine.Random.Range(0,weapon_list.Length);
		int rndt = UnityEngine.Random.Range(0,team_list.Length-1);
		
		int crit = 0;
		if (UnityEngine.Random.value < 0.1f) {
			crit = 1;
		}
		
		GameObject clone = GameObject.Instantiate(feed_item, feed_list.transform);
		clone.SetActive(true);
		plateInfo info = clone.GetComponent<plateInfo>();
		info.Start();
		
		if (UnityEngine.Random.value < 0.05f) {
			if (type == "Arena") {
				if (UnityEngine.Random.value < 0.5f) type = "Capture the Flag";
				else type = "Control Point";
			}
			
			int rndo = UnityEngine.Random.Range(0,obj_icons.Length);
			string text = "";
			
			switch (type){
			case "Capture the Flag":
				if (rndo < 2) text = "picked up the intelligence!";
				else text = "defended the intelligence!";
				break;
			default:
				if (rndo < 2) text = "captured the Control Point";
				else text = "defended the Control Point";
				break;
			}
			
			if (rndo % 2 == 0) info.setPlayer(0, BLUTeam[rnd2], "BLU");
			else info.setPlayer(0, REDTeam[rnd1], "RED");

			info.setObjective(obj_icons[rndo], text);
		} else {
			
			if (team_list[rndt] == "BLU") {
				info.setPlayer(1, REDTeam[rnd1], "RED");
				info.setPlayer(0, BLUTeam[rnd2], "BLU");
			} else if (team_list[rndt] == "RED") {
				info.setPlayer(0, REDTeam[rnd1], "RED");
				info.setPlayer(1, BLUTeam[rnd2], "BLU");
			} 
		
			info.setWeapon(weapon_list[rndw], icons[rndw], crit);
		}
		
	}
	
	void Awake () {
		solveb.OnInteract += delegate () { module.HandlePass(); return false; };	
		foreach ( KMSelectable answerb in answer_list ) {
			Transform answer_obj = answerb.transform.GetChild(0).transform.GetChild(0);
			answerb.OnInteract += delegate () { AnswerPress(answer_obj.GetComponent<UnityEngine.UI.Text>().text); return false; };	
		}
		
		moduleId = moduleCount++;
	}
	
	void Update () {
		if (isSolved) return;
		
		if (stage != bomb.GetSolvedModuleNames().ToList().Count && state == "start") {
			feed_item.SetActive(false);
			stage++;
			
			//scoreboard update
			foreach (Transform child in feed_list.transform) {
				plateInfo plate = child.GetComponent<plateInfo>();
				switch (plate.playerTeams[0]) {
				case "BLU":
					team_scores[0]++;
					break;
				case "RED":
					team_scores[1]++;
					break;
				}
				
				if (plate.playerNames[1] != "") {
					kill_scores[plate.playerNames[0]]++;
					domination_count[plate.playerNames[0]][plate.playerNames[1]]++;
					kill_tracker[plate.playerNames[0]][plate.playerNames[1]]++;
					if (domination_count[plate.playerNames[1]][plate.playerNames[0]] >= 4) kill_scores[plate.playerNames[0]]++;
					domination_count[plate.playerNames[1]][plate.playerNames[0]] = 0;
					death_scores[plate.playerNames[1]]++;
					
					string weapon = plate.weapon;
					if ( weapon == "Backstab" ) {
						weapon_scores[weapon]++;
						weapon = "Knife";
					}
					if ( weapon == "Sniper Rifle(HS)" ) {
						weapon_scores[weapon]++;
						weapon = "Sniper Rifle";
					}
					weapon_scores[weapon]++;
					
					Debug.LogFormat("[Spectator Sport #{0}] <Stage {1}> {2} ({3}) killed {4} ({5}) with the {6}.", moduleId, stage-1, plate.playerNames[0], plate.playerTeams[0], plate.playerNames[1], plate.playerTeams[1], weapon);
				} else Debug.LogFormat("[Spectator Sport #{0}] <Stage {1}> {2} ({3}) has completed the objective (Gamemode {4}).", moduleId, stage-1, plate.playerNames[0], plate.playerTeams[0], type);

				if (plate.iscrit) {
					crit_counter++;
					Debug.LogFormat("[Spectator Sport #{0}] <Stage {1}> Crit!", moduleId, stage-1);
				}
				GameObject.Destroy(child.gameObject);
				
			}
			
			if ( stage >= lastStage ) {
				//last stage question time
				question_item.SetActive(true);
				audio.PlaySoundAtTransform("Start", transform);
				GenerateQuestion();
				state = "question";
			} else {
				for ( int i = 0; i < UnityEngine.Random.Range(2, 5); i++){
					AddLine();
				}
			}
		}
	}
	
	void GenerateQuestion () {
		int question_id = UnityEngine.Random.Range(0, questions.Length);
		while (questions[question_id] == "Answered") {
			question_id = UnityEngine.Random.Range(0, questions.Length);
		}
		
		string[] answers = new string[3];
		switch (question_id) {
		case 0:
			kill_scores = kill_scores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
			correct = kill_scores.Keys.First();
			
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				answers[i] = kill_scores.Keys.ToList()[i];
				int rnda = 6; //8
				int loop = 0;
				while (answers.Contains(kill_scores.Keys.ToList()[rnda]) || kill_scores[correct] == kill_scores[answers[i]]) {
					rnda = UnityEngine.Random.Range(0,kill_scores.Keys.Count);
					answers[i] = kill_scores.Keys.ToList()[rnda];
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
			}
			break;
		case 1:
			death_scores = death_scores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
			correct = death_scores.Keys.First();
			
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				answers[i] = death_scores.Keys.ToList()[i];
				int rnda = 6; //8
				int loop = 0;
				while (answers.Contains(death_scores.Keys.ToList()[rnda]) || death_scores[correct] == death_scores[answers[i]]) {
					rnda = UnityEngine.Random.Range(0,death_scores.Keys.Count);
					answers[i] = death_scores.Keys.ToList()[rnda];
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
			}
			break;
		case 2:
			answers = new string[] {"RED", "BLU", "Tie"};
			foreach (string answer in answers) {
				int i = System.Array.IndexOf(answers, answer);
				answer_list[i].transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = answer;
			}
			if (team_scores[0] > team_scores[1]) correct = "BLU";
			else if (team_scores[0] < team_scores[1]) correct = "RED";
			else correct = "Tie";
			break;
		case 3:
			answers = new string[] {"Arena", "Capture the Flag", "Control Point"};
			correct = type;
			break;
		case 4:
			weapon_scores = weapon_scores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
			correct = weapon_scores.Keys.First();
			
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				int rnda = UnityEngine.Random.Range(0,weapon_scores.Keys.Count);
				int loop = 0;
				while (answers.Contains(weapon_scores.Keys.ToList()[rnda]) || weapon_scores[correct] == weapon_scores[weapon_scores.Keys.ToList()[rnda]]) {
					rnda = UnityEngine.Random.Range(0,weapon_scores.Keys.Count);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i] = weapon_scores.Keys.ToList()[rnda];
			}
			break;
		case 5:
			correct = crit_counter.ToString();
			
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				if (crit_counter < 2) crit_counter = 2;
				int rnda = UnityEngine.Random.Range(crit_counter - 2, crit_counter + 4);
				int loop = 0;
				while (answers.Contains(rnda.ToString())) {
					rnda = UnityEngine.Random.Range(crit_counter - 2, crit_counter + 4);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i]= rnda.ToString();
				
			}
			break;
		case 6:
		case 8:
			string[] allplayers = REDTeam.Concat(BLUTeam).ToArray();
			string selection = allplayers[UnityEngine.Random.Range(0,allplayers.Length)];
			questions[question_id] = string.Format("How many players did {0} manage to deprive of life?", selection);
			
			correct = kill_scores[selection].ToString();
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				if (kill_scores[selection] < 2) kill_scores[selection] = 2;
				int rnda = UnityEngine.Random.Range(kill_scores[selection] - 2, kill_scores[selection] + 4);
				int loop = 0;
				while (answers.Contains(rnda.ToString())) {
					rnda = UnityEngine.Random.Range(kill_scores[selection] - 2, kill_scores[selection] + 4);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i]= rnda.ToString();
				
			}
			break;
		case 7:
		case 9:
			allplayers = REDTeam.Concat(BLUTeam).ToArray();
			selection = allplayers[UnityEngine.Random.Range(0,allplayers.Length)];
			questions[question_id] = string.Format("How many times did {0} undergo the permanent cessation of all vital functions?", selection);
			
			correct = death_scores[selection].ToString();
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				if (death_scores[selection] < 2) death_scores[selection] = 2;
				int rnda = UnityEngine.Random.Range(death_scores[selection] - 2, death_scores[selection] + 4);
				int loop = 0;
				while (answers.Contains(rnda.ToString())) {
					rnda = UnityEngine.Random.Range(death_scores[selection] - 2, death_scores[selection] + 4);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i]= rnda.ToString();
				
			}
			break;
		case 10:
			//Which player managed to get the least amount of kills in the round?
		
			kill_scores = kill_scores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
			correct = kill_scores.Keys.Last();
			answers[0] = correct;
			answers[1] = kill_scores.Keys.ToList()[0];
			answers[2] = kill_scores.Keys.ToList()[1];
			break;
		case 11:
		case 12:
		
			selection = weapon_list[UnityEngine.Random.Range(0,weapon_list.Length)];
			questions[question_id] = string.Format("How many times was the {0} used to kill an enemy player?", selection);
			
			correct = weapon_scores[selection].ToString();
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				if (weapon_scores[selection] < 2) weapon_scores[selection] = 2;
				int rnda = UnityEngine.Random.Range(weapon_scores[selection] - 2, weapon_scores[selection] + 4);
				int loop = 0;
				while (answers.Contains(rnda.ToString())) {
					rnda = UnityEngine.Random.Range(weapon_scores[selection] - 2, weapon_scores[selection] + 4);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i] = rnda.ToString();
			}
			break;
		case 13:
			//How many times did player {A} kill player {B}?
			allplayers = REDTeam.Concat(BLUTeam).ToArray();
			selection = allplayers[UnityEngine.Random.Range(0,allplayers.Length)];
			string victim = kill_tracker[selection].Keys.ToList()[UnityEngine.Random.Range(0,7)];
			questions[question_id] = string.Format("How many times did {0} manage to beat {1} in a trial by combat?", selection, victim);
			
			correct = kill_tracker[selection][victim].ToString();
			answers[0] = correct;
			for (int i = 1; i < 3; i++) {
				if (kill_tracker[selection][victim] < 2) kill_tracker[selection][victim] = 2;
				int rnda = UnityEngine.Random.Range(kill_tracker[selection][victim] - 2, kill_tracker[selection][victim] + 4);
				int loop = 0;
				while (answers.Contains(rnda.ToString())) {
					rnda = UnityEngine.Random.Range(kill_tracker[selection][victim] - 2, kill_tracker[selection][victim] + 4);
					loop++;
					if (loop >= 100) {
						Debug.LogFormat("Not possible to generate question");
						GenerateQuestion();
						return;
					}
				}
				answers[i] = rnda.ToString();
				
			}
			break;
		}
		
		
		Debug.LogFormat("[Spectator Sport #{0}] <Final Stage> Question: {1}. Correct answer is: {2}.", moduleId, questions[question_id], correct);
		question_text.text = questions[question_id];
		questions[question_id] = "Answered";
		Shuffle(answers);
		
		foreach (string answer in answers) {
			int i = System.Array.IndexOf(answers, answer);
			answer_list[i].transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = answer;
		}
	}
	
	void AnswerPress ( string ans ) {
		if ( ans == correct ) {
			solves++;
			audio.PlaySoundAtTransform("Correct" + UnityEngine.Random.Range(1, 5).ToString(), transform);
			if (solves >= 5) Solve();
			else GenerateQuestion();
		}
		else {
			module.HandleStrike();
			audio.PlaySoundAtTransform("Strike" + UnityEngine.Random.Range(1, 7).ToString(), transform);
		}
	}
	
	void Solve () {
		isSolved = true;
		question_item.SetActive(false);
		feed_item.SetActive(true);
		
		feed_item.transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Victory!";
		audio.PlaySoundAtTransform("Solve", transform);
		
		module.HandlePass();
	}
	
	void Shuffle( string[] arr ) {
		for (int t = 0; t < arr.Length; t++ )
        {
            string tmp = arr[t];
            int r = Random.Range(t, arr.Length);
            arr[t] = arr[r];
            arr[r] = tmp;
        }
	}
	
	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} select <1/2/3> [Selects the specified answer from top to bottom]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (parameters[0].EqualsIgnoreCase("select"))
		{
			if (parameters.Length > 2)
				yield return "sendtochaterror Too many parameters!";
			else if (parameters.Length == 1)
				yield return "sendtochaterror Please specify an answer to select!";
			else
			{
				int temp;
				if (!int.TryParse(parameters[1], out temp))
				{
					yield return "sendtochaterror!f The specified answer '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (temp < 1 || temp > 3)
				{
					yield return "sendtochaterror The specified answer '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (state != "question")
				{
					yield return "sendtochaterror The module is not in the questioning phase yet!";
					yield break;
				}
				yield return null;
				answer_list[temp - 1].OnInteract();
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		while (state != "question") yield return true;
		while (!isSolved)
		{
			for (int i = 0; i < answer_list.Length; i++)
			{
				if (answer_list[i].transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text == correct)
				{
					answer_list[i].OnInteract();
					yield return new WaitForSeconds(.1f);
					break;
				}
			}
		}
	}
}

