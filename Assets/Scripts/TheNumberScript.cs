using UnityEngine;
using TheNumber;
using System.Collections.Generic;
using System.Linq;
using Rnd = UnityEngine.Random;
using System;
using System.Text.RegularExpressions;
using System.Collections;

public class TheNumberScript : MonoBehaviour
{
	#region Global Variables
	public KMBombInfo Info;
	public KMAudio Audio;
	public KMBombModule Module;
	
	public KMSelectable[] Buttons;
	public TextMesh[] ButtonTexts;
	public TextMesh Screen;
	public KMSelectable CancelButton, SubmitButton;

	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;
	private int StartTime;
	private readonly int CurrentTime;
	private List<string> ModulesName;
	private DayOfWeek day;
	private List<int> RandomSelected = new List<int> { };
	private List<int> FirstRow = new List<int> { };
	private List<int> SecondRow = new List<int> { };

	private string InputString = "";
	private int Number1, Number2, Number3, Number4;
	private int stage = 1, input = 0;
	private int sequence = 0;
	private int count1 = 0, count2 = 0;
	private bool ordered = false, contains = false;

	private bool _isSolved = false, _lightsOn = false, Started = false, Strike = false, ForcedSolve = false;
	#endregion

	#region Answer Calculation
	void Start()
	{
		_moduleId = _moduleIdCounter++;
		Module.OnActivate += Activate;
	}

	void Activate()
	{
		Init();
		_lightsOn = true;
	}

	void Init()
	{
		//strike
		if (Strike)
		{
			Screen.text = "";
			input = 0;
			count1 = 0;
			count2 = 0;
			ordered = false;
			contains = false;
			stage = 1;
			InputString = "";
			Strike = false;
		}
		if (!Started)
		{
			StartTime = Mathf.FloorToInt(Info.GetTime());
			day = DateTime.Now.DayOfWeek;
			ModulesName = Info.GetModuleNames();
			//rules must be calculated at submit button press
		}
		RandomSelected.Clear();
		FirstRow.Clear();
		SecondRow.Clear();
		RandomiseNumbers();
		NumberCalculations();
		Debug.LogFormat("[The Number #{0}] First row is {1}, {2}, {3}, {4} and {5}", _moduleId, FirstRow[0], FirstRow[1], FirstRow[2], FirstRow[3], FirstRow[4]);
		Debug.LogFormat("[The Number #{0}] Second row is {1}, {2}, {3}, {4} and {5}", _moduleId, SecondRow[0], SecondRow[1], SecondRow[2], SecondRow[3], SecondRow[4]);
	}

	private void NumberCalculations()
	{
		//count1
		for (int i = 0; i <= 4; i++)
		{
			int buttontext = RandomSelected[i];
			if (buttontext % 2 == 1)
				count1++;
		}
		//count2
		for (int i = 5; i <= 9; i++)
		{
			int buttontext = RandomSelected[i];
			if (buttontext % 2 == 1)
				count2++;
		}
		//ordered
		if ((RandomSelected[4] == RandomSelected[3] + 1 && RandomSelected[3] == RandomSelected[2] + 1
			&& RandomSelected[2] == RandomSelected[1] + 1 && RandomSelected[1] == RandomSelected[0] + 1) ||
			(RandomSelected[9] == RandomSelected[8] + 1 && RandomSelected[8] == RandomSelected[7] + 1
			&& RandomSelected[7] == RandomSelected[6] + 1 && RandomSelected[6] == RandomSelected[5] + 1))
			ordered = true;
		//lists
		for (int i = 0; i <= 4; i++)
			FirstRow.Add(RandomSelected[i]);
		for (int i = 5; i <= 9; i++)
			SecondRow.Add(RandomSelected[i]);
		//contains
		if ((FirstRow.Contains(0) && FirstRow.Contains(1) && FirstRow.Contains(7) &&
			FirstRow.Contains(8) && FirstRow.Contains(9)) || (SecondRow.Contains(0) && SecondRow.Contains(1) &&
			SecondRow.Contains(7) && SecondRow.Contains(8) && SecondRow.Contains(9)))
			contains = true;
	}

	private void RandomiseNumbers()
	{
		foreach (TextMesh Mesh in ButtonTexts)
		{
			bool valid = false;
			while (!valid)
			{
				int number = Rnd.Range(0, 10);
				if (RandomSelected.Contains(number)) continue;
				Mesh.text = number.ToString();
				RandomSelected.Add(number);
				valid = true;
			}
		}
	}

	private void ClearNumbers()
	{
		for (int i = 0; i <= 9; i++)
		{
			ButtonTexts[i].text = i.ToString();
		}
	}

	private List<string> RemoveSolved()
	{
		List<string> solved = Info.GetSolvedModuleNames();
		List<string> answer = new List<string> { };
		answer.AddRange(ModulesName);
		foreach (string module in solved)
		{
			answer.Remove(module);
		}

		return answer;
	}

	private void RunRules()
	{
		List<string> removed = RemoveSolved();
		//First Number
		if (Info.IsTwoFactorPresent())
		{
			Number1 = 7;
			Debug.LogFormat("[The Number #{0}] First number is a 7 (two factor present)", _moduleId);
		} else if (Info.GetBatteryHolderCount() >= 3)
		{
			Number1 = 0;
			Debug.LogFormat("[The Number #{0}] First number is a 0 (battery holders)", _moduleId);
		} else if (Info.GetPortPlates().Any(x => x.Length == 0))
		{
			Number1 = 9;
			Debug.LogFormat("[The Number #{0}] First number is a 9 (empty port plate)", _moduleId);
		} else if (!Info.IsDuplicatePortPresent())
		{
			Number1 = 5;
			Debug.LogFormat("[The Number #{0}] First number is a 5 (unique ports)", _moduleId);
		} else if (Info.GetBatteryCount() == 0)
		{
			Number1 = 3;
			Debug.LogFormat("[The Number #{0}] First number is a 3 (no batteries)", _moduleId);
		} else if (Info.GetSerialNumber().Any("OMZ6L5".Contains))
		{
			Number1 = 1;
			Debug.LogFormat("[The Number #{0}] First number is a 1 (OMZ6L5)", _moduleId);
		} else if (Info.GetBatteryCount() < (Info.GetSolvableModuleNames().Count - Info.GetSolvedModuleNames().Count))
		{
			Number1 = 6;
			Debug.LogFormat("[The Number #{0}] First number is a 6 (unsolved modules)", _moduleId);
		} else if (Info.GetOnIndicators().Count() >= 2)
		{
			Number1 = 8;
			Debug.LogFormat("[The Number #{0}] First number is an 8 (lit indicators)", _moduleId);
		} else if (Info.GetOffIndicators().Count() == 1)
		{
			Number1 = 2;
			Debug.LogFormat("[The Number #{0}] First number is a 2 (unlit undicator)", _moduleId);
		} else
		{
			Number1 = 4;
			Debug.LogFormat("[The Number #{0}] First number is a 4 (otherwise)", _moduleId);
		}
		//Second Number
		if (count1 >= 3)
		{
			Number2 = 2;
			Debug.LogFormat("[The Number #{0}] Second number is a 2 (odd numbers)", _moduleId);
		} else if (ordered)
		{
			Number2 = 9;
			Debug.LogFormat("[The Number #{0}] Second number is a 9 (ordered)", _moduleId);
		} else if (SecondRow.Sum() > 16)
		{
			Number2 = 8;
			Debug.LogFormat("[The Number #{0}] Second number is an 8 (second row greater than 16)", _moduleId);
		} else if (FirstRow.Sum() < 15)
		{
			Number2 = 3;
			Debug.LogFormat("[The Number #{0}] Second number is a 3 (second row less than 15)", _moduleId);
		} else if (RandomSelected[2] % 2 == RandomSelected[7] % 2)
		{
			Number2 = 0;
			Debug.LogFormat("[The Number #{0}] Second number is a 0 (third column both even or odd)", _moduleId);
		} else if (FirstRow.Contains(2) && FirstRow.Contains(4) && FirstRow.Contains(7))
		{
			Number2 = 5;
			Debug.LogFormat("[The Number #{0}] Second number is a 5 (2 4 and 7)", _moduleId);
		} else if (count2 == 2)
		{
			Number2 = 1;
			Debug.LogFormat("[The Number #{0}] Second number is a 1 (odd numbers is 2)", _moduleId);
		} else if (contains)
		{
			Number2 = 6;
			Debug.LogFormat("[The Number #{0}] Second number is a 6 (0 1 7 8 and 9)", _moduleId);
		} else if (SecondRow.Contains(7))
		{
			Number2 = 7;
			Debug.LogFormat("[The Number #{0}] Second number is a 7 (7 in the second row)", _moduleId);
		} else
		{
			Number2 = 4;
			Debug.LogFormat("[The Number #{0}] Second number is a 4 (otherwise)", _moduleId);
		}
		//Third number
		if (Info.GetSolvedModuleNames().Count == 7)
		{
			Number3 = 7;
			Debug.LogFormat("[The Number #{0}] Third number is a 7 (solved modules is 7)", _moduleId);
		} else if (ModulesName.Count == 9)
		{
			Number3 = 9;
			Debug.LogFormat("[The Number #{0}] Third number is a 9 (number of modules is 9)", _moduleId);
		} else if (ModulesName.Contains("The Gamepad") || ModulesName.Contains("Number Pad"))
		{
			Number3 = 6;
			Debug.LogFormat("[The Number #{0}] Third number is a 6 (Gamepad or Numberpad)", _moduleId);
		} else if (((int)(StartTime / 60.0f)) < ModulesName.Count)
		{
			Number3 = 0;
			Debug.LogFormat("[The Number #{0}] Third number is a 0 (start time less than number of modules)", _moduleId);
		} else if (Info.GetSolvedModuleNames().Count > (ModulesName.Count - Info.GetSolvedModuleNames().Count))
		{
			Number3 = 1;
			Debug.LogFormat("[The Number #{0}] Third number is a 1 (solved greater than unsolved", _moduleId);
		} else if (Info.GetSolvedModuleNames().Contains("Timezone") || Info.GetSolvedModuleNames().Contains("The Bulb") || Info.GetSolvedModuleNames().Contains("Semaphore"))
		{
			Number3 = 2;
			Debug.LogFormat("[The Number #{0}] Third number is a 2 (Timezones, The Bulb or Semaphore", _moduleId);
		} else if (removed.Contains("Cryptography") || removed.Contains("Light Cycle") || removed.Contains("Piano Keys"))
		{
			Number3 = 8;
			Debug.LogFormat("[The Number #{0}] Third number is an 8 (Cryptography, Light Cycle or Piano Keys)", _moduleId);
		} else if (Info.GetStrikes() >= 1)
		{
			Number3 = 3;
			Debug.LogFormat("[The Number #{0}] Third number is a 3 (at least 1 strike)", _moduleId);
		} else if (ModulesName.Count - Info.GetSolvableModuleNames().Count > 0)
		{
			Number3 = 5;
			Debug.LogFormat("[The Number #{0}] Third number is a 5 (needy module)", _moduleId);
		} else
		{
			Number3 = 4;
			Debug.LogFormat("[The Number #{0}] Third number is a 4 (otherwise)", _moduleId);
		}
		//Fourth number
		if (day == DayOfWeek.Monday || day == DayOfWeek.Wednesday || day == DayOfWeek.Friday)
		{
			Number4 = 1;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 1 (Monday, Wednesday, Friday)", _moduleId);
		}
		else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour < 17)
		{
			Number4 = 0;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 0 (between 12 and 17)", _moduleId);
		}
		else if (Number1 % 2 == 1 && Number3 % 2 == 1)
		{
			Number4 = 8;
			Debug.LogFormat("[The Number #{0}] Fourth number is an 8 (first and third odd)", _moduleId);
		} else if (ModulesName.Contains("Forget Me Not"))
		{
			Number4 = 9;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 9 (Forget Me Not)", _moduleId);
		} else if (Info.GetPorts().GroupBy((x) => x).Any((y) => y.Count() >= 3))
		{
			Number4 = 7;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 7 (three or more duplicated ports)", _moduleId);
		} else if (Number1 * Number2 * Number3 > 100)
		{
			Number4 = 5;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 5 (multiply to greater than 100)", _moduleId);
		} else if (Number1 + Number2 + Number3 > 19)
		{
			Number4 = 3;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 3 (add to greater than 19)", _moduleId);
		} else if (Number1 == 2 || Number2 == 2 || Number3 == 2)
		{
			Number4 = 2;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 2 (previous contains a two)", _moduleId);
		} else if (Info.GetTime() < StartTime / 2.0f)
		{
			Number4 = 6;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 6 (less than half of the initial starting time)", _moduleId);
		} else
		{
			Number4 = 4;
			Debug.LogFormat("[The Number #{0}] Fourth number is a 4 (otherwise)", _moduleId);
		}

		sequence = Number1 * 1000 + Number2 * 100 + Number3 * 10 + Number4;
		Debug.LogFormat("[The Number #{0}] Final sequence is {1}", _moduleId, sequence);
		Started = true;
	}
	#endregion

	#region Button Handling
	private void Awake()
	{
		CancelButton.OnInteract += delegate ()
		{
			CancelHandler();
			return false;
		};
		SubmitButton.OnInteract += delegate ()
		{
			SubmitHandler();
			return false;
		};
		for (int i = 0; i < 10; i++)
		{
			int b = i;
			Buttons[i].OnInteract += delegate ()
			{
				ButtonHandler(b);
				return false;
			};
		}
	}

	private void CancelHandler()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, CancelButton.transform);
		CancelButton.AddInteractionPunch();

		if (!_lightsOn || _isSolved) return;
		input = 0;
		Screen.text = "";
		stage = 1;
		InputString = "";
	}

	private void SubmitHandler()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);
		SubmitButton.AddInteractionPunch();

		if (!_lightsOn || _isSolved) return;
		if (!ForcedSolve)
		{
			Debug.LogFormat("[The Number #{0}] Submit pressed, Running Rules", _moduleId);

			RunRules();
		}
		switch (stage)
		{
			case 2: //all these are +1 due to the stage being incremented after the value is noted
				input /= 1000;
				break;
			case 3:
				input /= 100;
				break;
			case 4:
				input /= 10;
				break;
			default:
				break;
		}

		Debug.LogFormat("[The Number #{0}] Received {1}. Expected {2}", _moduleId, input, sequence);

		if (input == sequence)
		{
			Debug.LogFormat("[The Number #{0}] Module Passed", _moduleId);
			Module.HandlePass();
			Screen.text = "";
			ClearNumbers();
			_isSolved = true;
		} else
		{
			Debug.LogFormat("[The Number #{0}] Strike, Reset module", _moduleId);
			Module.HandleStrike();
			Strike = true;
			Init();
		}
	}

	private void ButtonHandler(int b)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[b].transform);
		Buttons[b].AddInteractionPunch();

		if (!_lightsOn || _isSolved) return;


		switch (stage)
		{
			case 1:
				input += RandomSelected[b] * 1000;
				InputString += RandomSelected[b].ToString();
				stage++;
				break;
			case 2:
				input += RandomSelected[b] * 100;
				InputString += RandomSelected[b].ToString();
				stage++;
				break;
			case 3:
				input += RandomSelected[b] * 10;
				InputString += RandomSelected[b].ToString();
				stage++;
				break;
			case 4:
				input += RandomSelected[b];
				InputString += RandomSelected[b].ToString();
				stage++;
				break;
			default:
				break;
		}
		Screen.text = InputString;
	}
	#endregion

	#region Twitch Plays
#pragma warning disable 414
	private string TwitchHelpMessage = "Use '!{0} press 5 4 9 10 submit' to press the buttons in the positions 5, 4, 9 and 10 and the submit button. The positions are numbered from 1-10 with 1 being the top left, proceeding in reading order. '!{0} submit' and '!{0} cancel' are also accepted";
#pragma warning restore 414
	KMSelectable[] ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant().Trim();
		if (Regex.IsMatch(command, @"^press (.+)"))
		{
			bool valid = true;
			string[] split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in split.Skip(1))
			{
				valid &= (item.Contains("submit") || item.Contains("s") || item.Contains("c") || item.Contains("cancel") || item.Contains("e") || item.Any("123456789".Contains) || item.Contains("10"));
			}
			if (valid)
			{
				List<KMSelectable> buttons = new List<KMSelectable> { };
				foreach (string item in split.Skip(1))
				{
					switch (item)
					{
						case "submit":
						case "s":
						case "e":
						case "enter":
							buttons.Add(SubmitButton);
							break;
						case "c":
						case "cancel":
						case "clear":
							buttons.Add(CancelButton);
							break;
						case "1":
						case "2":
						case "3":
						case "4":
						case "5":
						case "6":
						case "7":
						case "8":
						case "9":
						case "10":
							int itemint;
							int.TryParse(item, out itemint);
							itemint--;
							buttons.Add(Buttons[itemint]);
							break;
						default:
							return null;
					}
				}
				return buttons.ToArray();
			}
			else
				return null;
		}
		else if (command.Equals("submit") || command.Equals("e") || command.Equals("s") || command.Equals("enter"))
		{
			return new KMSelectable[] { SubmitButton };
		}
		else if (command.Equals("cancel") || command.Equals("c") || command.Equals("clear"))
		{
			return new KMSelectable[] { CancelButton };
		}
		else
			return null;
	}
	private IEnumerator TwitchHandleForcedSolve()
	{
		if (!_isSolved)
		{
			yield return null;
			Debug.LogFormat("[The Number #{0}] Module forcibly solved", _moduleId);
			ForcedSolve = true;
			RunRules();

			IEnumerable<int> SequenceEnumerable = sequence.ToString().Select(digit => int.Parse(digit.ToString()));
			foreach (int digit in SequenceEnumerable)
			{
				int index = RandomSelected.IndexOf(digit);
				ButtonHandler(index);
				yield return new WaitForSeconds(0.1f);
			}
			SubmitHandler();
		}
	}
	#endregion
}
