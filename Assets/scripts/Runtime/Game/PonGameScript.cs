// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening.Core;
using UnityEngine.Audio;
using System;
using System.Text.RegularExpressions;

namespace Pon
{
	/// <summary>
	/// Create grid/players and tie everything together
	/// </summary>
	public class PonGameScript : MonoBehaviour
	{
		[Header("Inventory")]
		public InGameInventory inGameInventory;
		private List<Item> inventoryItems;

        private static GameObject DOTweenGameObject;
		static PonGameScript()
		{
			UnityLog.Init();
		}

		public static PonGameScript instance;

		#region Members

		[Header("Prefabs")]
		public CursorScript[] cursorPrefabs;

		private GameSettings settings;
		private Objective objectives;
		private List<PlayerScript> players = new List<PlayerScript>();

		private bool isPaused, isOver;
		private float timeElapsed;

		private bool firstGridStarted;
		private int maxStressLevel;

		[SerializeField] private AudioSource musicSource;
		[SerializeField] private AudioClip winMusic;
		[SerializeField] private AudioClip loseMusic;

        private bool lostByFillingScreen = false;
		private bool wonGame = false;
    public bool isTutorial = false;

		private bool incPowerFillSpeed = false;
		private float incPowerFillSpeedPerc = 0;

        #endregion

        #region Timeline

        string currentLevelName;
		PlayerScript player;
		private void Awake()
		{
			instance = this;

			if (MapUIScript.mapInstance != null)
			{
				// set game settings based on level
				currentLevelName = MapUIScript.mapInstance.currentLevelName;
			}
			

			musicSource = GameObject.FindGameObjectWithTag("MusicSource").GetComponent<AudioSource>();
		}

        private void Start()
		{

			// Threads
			Loom.Initialize();

			GetSettings();

            if (InGameInventory.Instance != null)
            {
                inGameInventory = InGameInventory.Instance;
                inGameInventory.LoadInventory();
                inventoryItems = inGameInventory.GetItems();
                ApplyItemEffects(inventoryItems);
            }
            else
            {
                Debug.Log("Inventory is null :(");
            }

            PrepareUI();

			CreatePlayersAndGrids();

			StartGrids();

            if (DOTweenGameObject == null)
			{
				DOTweenGameObject = GameObject.Find("[DOTween]");
			}
			else
			{
				DestroyImmediate(DOTweenGameObject);
				DOTweenGameObject = new GameObject("[DOTween]");
				DOTweenGameObject.AddComponent<DOTweenComponent>();
			}

			GoogleAnalyticsHelper.AnalyticsLevelStart(currentLevelName);

			/*
			Firebase.Analytics.FirebaseAnalytics.LogEvent(
			   Firebase.Analytics.FirebaseAnalytics.EventLevelStart,
			   new Firebase.Analytics.Parameter[] {
				new Firebase.Analytics.Parameter(
				  Firebase.Analytics.FirebaseAnalytics.ParameterLevel, 1),

			   }

			 );
			*/

		}

		private void OnDestroy()
		{
			foreach (var p in players)
			{
				if (p != null && p.power != null)
				{
					p.power.OnPowerUsed -= OnPowerUsed;
				}
			}
		}

		void Update()
		{
			if (isPaused == false && isOver == false)
			{
				timeElapsed += Time.deltaTime;
			}

			// player runs out of time (objectives has a time limit set under "Time Reached")
			if (objectives != null)
			{
				if (objectives.stats.timeMax <= timeElapsed && objectives.stats.timeMax != 0 && lostByFillingScreen == false && wonGame == false)
				{
					musicSource.PlayOneShot(loseMusic);
					player.grid.SetGameOver();
					DOVirtual.DelayedCall(3f, TriggerGameOver);

					// prevents Update() from going into this if statement more than once (losemusic will play)
					objectives.stats.timeMax = 0;

					// stops timer
					isOver = true;
				}
			}

			GameUIScript.SetTime(timeElapsed);

			// Update objectives
			if (objectives != null)
			{
				UpdateObjectives();
			}
		}

		private void UpdateObjectives()
		{
			foreach (var p in players)
			{
				if (p.grid.IsPaused) continue;

				// Give new stats, widget will check if it's relevant
				var currentStats = new ObjectiveStats(p, timeElapsed);

				for (int i = 1; i <= 6; i++)
				{
					GameUIScript.UpdateObjective(p, i, currentStats);
				}
			}

			var p1 = players[0];

			// Stop here if not started
			if (p1.grid.IsStarted == false) return;
			//--------------------------------------------------------------------------------------------------------------

			if (isOver == false)
			{
				for (int i = 0; i < players.Count; i++)
				{
					var pWinner = players[i];
					var pStats = new ObjectiveStats(pWinner, timeElapsed);
					if (objectives.Succeed(pWinner, pStats))
					{
						Log.Info("Versus with level ended!");

						// WIN MUSIC (need to delay level from ending until music is done)
						musicSource.PlayOneShot(winMusic);
						// Invoke("test", 10f);
						// StartCoroutine(MyFunction(5f, pWinner));
						GameOverVersus(pWinner);
						// make sure timer stops when you win
						wonGame = true;
						break;
					}
				}
			}
		}


		private void GameOverVersus(PlayerScript winner)
		{
			// One player wins
			isOver = true;

			var sequence = DOTween.Sequence();
			sequence.Append(DOTween.To(() => winner.grid.ScaleTime, (v) =>
			  {
				  foreach (var p in players)
				  {
					  p.grid.ScaleTime = v;
				  }
			  }, 0f, 1f)
			  .SetEase(Ease.OutCubic));
			sequence.AppendInterval(1f);
			sequence.AppendCallback(() =>
			{
				winner.player.GameOver = false;
				winner.grid.SetVictory();

				foreach (var pLoser in players)
				{
					if (pLoser == winner) continue;
					pLoser.player.GameOver = true;
					pLoser.grid.SetGameOver();
				}
			});
			sequence.AppendInterval(2f);
			sequence.AppendCallback(TriggerGameOver);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Find game settings (players, grid size, grid speed, etc) or create default ones.
		/// </summary>
		private void GetSettings()
		{
			settings = FindObjectOfType<GameSettings>();
			if (settings == null)
			{
				// Default
				Log.Error("Missing game settings. Please add one to the scene.");
				return;
			}

			if (settings.players.Length == 0)
			{
				Log.Error("No player defined... Nothing is going to happen!");
				return;
			}

			if (settings.players.Length == 1)
			{
				//Log.Error("Needs 2 players.");  We need to allow just one player
				settings.playMode = PlayMode.Singleplayer; // Change playmode to singleplayer checks later
			}

			if (settings.enableObjectives)
			{
				objectives = settings.objective.Clone();
				if (objectives == null)
				{
					Log.Error("Missing objectives");
					return;
				}
			}
		}

		private void PrepareUI()
		{
			GameUIScript.Init(settings);
			GameUIScript.SetPlayersCount(settings.players.Length, settings.players.Count(p => p.type == PlayerType.AI));

			var ui = GameUIScript.GetUI();
			for (int i = 0;
			  i < settings.players.Length;
			  i++)
			{
				var p = settings.players[i];

				var z = ui.GetPlayerZone(p);
				p.gridViewport = z.rect;

				if (p.allowGridAngle)
				{
					p.gridAngle = z.angle;
				}

				GameUIScript.SetScore(i, 0);
				GameUIScript.SetSpeed(i, settings.gridSettings.startLevel);


				if (objectives != null)
				{
					GameUIScript.SetObjective(p.index, 1, objectives);
				}
			}
		}

		/// <summary>
		/// Create players and related grid using game settings.
		/// </summary>
		private void CreatePlayersAndGrids()
		{
			foreach (var basePlayer in settings.players)
			{
				var p = basePlayer;
				var po = new GameObject();


				if (p.type == PlayerType.Local)
				{
					player = po.AddComponent<PlayerScript>();
				}
				else if (p.type == PlayerType.AI)
				{
					player = po.AddComponent<AIPlayerScript>();
				}
				else
				{
					Log.Error("Unsupported player type " + p.type);
					return;
				}

				po.name = "Player " + p.name;
				po.transform.parent = transform;
				po.transform.position = Vector3.zero;
				player.player = p;
				if (p.power != PowerType.None)
				{
					player.power = Power.Create(p.power);
					player.power.OnPowerUsed += OnPowerUsed;
				}

				player.cursorPrefabs = cursorPrefabs;
				player.cam = GetCamera(player);
				player.grid = CreateGrid(player, player.cam);

				if (incPowerFillSpeed)
				{
					player.grid.IncreasePowerFillSpeed(incPowerFillSpeedPerc);
                }

                players.Add(player);

				// Init UI with player
				player.grid.ui = GameUIScript.SetPlayer(player, settings.players.Length);
				if (player.power != null)
				{
					GameUIScript.SetPowerCharge(p.index, 0, 0);
					player.grid.PowerCharge += 0.35f; // Start at n% > 0
				}
			}
		}

		/// <summary>
		/// Create a new camera for the player
		/// </summary>
		private Camera GetCamera(PlayerScript p)
		{
			var camGo = new GameObject("Camera P" + p.player.index);
			camGo.transform.position = new Vector3(p.transform.position.x, 0, -10);
			camGo.transform.parent = p.transform;

			var cam = camGo.AddComponent<Camera>();

			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.backgroundColor = Color.black;
			// Render everything *except* layer UI
			cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
			//cam.orthographicSize = 0: // This is not where we set the size. See GridScript.SetViewport.
			cam.orthographic = true;
			cam.depth = -5;

			return cam;
		}

		/// <summary>
		/// Create a grid for a player.
		/// </summary>
		private GridScript CreateGrid(PlayerScript playerScript, Camera cam)
		{
			var gridObj = new GameObject("Grid");
			gridObj.transform.parent = playerScript.transform;
			gridObj.transform.position = new Vector3(playerScript.player.index * 25, 0, 0);

			var grid = gridObj.AddComponent<GridScript>();
			grid.settings = settings.gridSettings.Clone();
			grid.viewportRect = playerScript.player.gridViewport;
			grid.angle = playerScript.player.gridAngle;
			grid.player = playerScript.player;
			grid.playerScript = playerScript;
			grid.gridCam = cam;
			grid.enablePower = (playerScript.player.power != PowerType.None);

			grid.OnGameOver += GameOver;
			grid.OnCombo += Combo;
			grid.OnScoreChanged += OnScoreChanged;
			grid.OnPowerChargeChanged += OnPowerChargeChanged;
			grid.OnLevelChanged += OnLevelChanged;
			grid.OnMultiplierChange += OnMultiplierChange;

            return grid;
		}

		public void StartGrids()
		{
			foreach (var playerScript in players)
			{
				playerScript.grid.IsStarted = true;
			}
		}

		private bool CheckCurrentCosmetic(List<Item> itemList)
        {
			foreach (Item item in itemList)
            {
				if (item != null && item.itemType == ItemType.Cosmetic)
                {
					if (item.isEnabled)
                    {
						return true; // if ANY cosmetic is currently enabled
                    }
                }
            }

			return false; // no cosmetics were found active
        }

        private void ApplyItemEffects(List<Item> itemList)
        {
			bool cosmeticIsApplied = CheckCurrentCosmetic(itemList);

			foreach (Item item in itemList)
			{
				switch (item.itemCodeName)
				{
					case "ComboFreezeIncre":
						if (item.isEnabled)
						{
							Debug.Log("Not implemented :( sorry ");
                        }
						break;
					case "ExpandBoardUpgrade":
						if (item.isEnabled)
						{
							if (settings == null)
							{
								Debug.Log("settings is null :(");
								break;
							}
							
							//Debug.Log("increasing width from " + settings.gridSettings.width + " to " + settings.gridSettings.width + 1);
							settings.gridSettings.width++;
                        }
                        break;
					case "HardModeUpgrade":
                        if (item.isEnabled)
                        {
							settings.gridSettings.width--;
							settings.gridSettings.previewLines--;
							settings.gridSettings.speedUpDuration = 0.4f;
							settings.currencyReward = (int) Math.Round(settings.currencyReward * 1.5f);
                        }
                        break;
					case "PowerFillSpeedIncre":
                        if (item.isEnabled)
                        {
							int amount = /*item.incLevel * 10*/ 30;
							incPowerFillSpeed = true;
							incPowerFillSpeedPerc = amount / 100;
                        }
                        break;
					case "SimplificatorPower":
                        if (item.isEnabled)
                        {
                            settings.players[0].power = PowerType.Simplificator;
                        }
                        break;
					case "TimeFreezePower":
                        if (item.isEnabled)
                        {
                            settings.players[0].power = PowerType.TimeFreeze;
                        }
                        break;

					// cosmetics
					case "CosmeticFruit":
						if (item.isEnabled)
                        {
							ApplyCosmetics(BlockDefinitionBank.Instance.fruitCosmetics); // fruits
						}
                        else // only switch back to default if NO COSMETICS APPLIED
                        {
							if (!cosmeticIsApplied) // cosmetic is not currently applied, can go back to default
                            {
								ApplyCosmetics(BlockDefinitionBank.Instance.defaultBlocks); // default
							}
						}
						break;
					case "CosmeticFlower":
						if (item.isEnabled)
						{
							ApplyCosmetics(BlockDefinitionBank.Instance.flowerCosmetics); // flowers
						}
						else
						{
							if (!cosmeticIsApplied) // cosmetic is not currently applied, can go back to default
							{
								ApplyCosmetics(BlockDefinitionBank.Instance.defaultBlocks); // default
							}
						}
						break;
					case "CosmeticCandy":
						if (item.isEnabled)
						{
							ApplyCosmetics(BlockDefinitionBank.Instance.candyCosmetics); // flowers
						}
						else
						{
							if (!cosmeticIsApplied) // cosmetic is not currently applied, can go back to default
							{
								ApplyCosmetics(BlockDefinitionBank.Instance.defaultBlocks); // default
							}
						}
						break;
				}
			}
        }

		public void ApplyCosmetics(BlockDefinition[] cosmetics)
        {
			// id to keep track of block type for objectives
			sbyte block_id = 1;
			// get the Data > Block Definition Bank Script > Definitions > Blocks
			if (BlockDefinitionBank.Instance != null)
			{
				for (int i = 0; i < 6; i++)
				{
					BlockDefinitionBank.Instance.definitions[i] = cosmetics[i];
					BlockDefinitionBank.Instance.definitions[i].id = block_id;
					block_id++;
				}
			}
		}

        #endregion

        #region Public methods

        public void SetPause(bool paused)
		{
			if (paused == false && isOver) return;

			isPaused = paused;

			foreach (var p in players)
			{
				p.grid.SetPause(paused);
			}
		}

		#endregion

		#region Events

		public void OnPowerUsed(Power power, PowerUseParams param)
		{
			// Dispatch to other players
			foreach (var p in players)
			{
				if (p.player.index != param.player.index && p.player.GameOver == false)
				{
					if (power.CanUseOnOpponent(p.grid, p.grid.TheGrid))
					{
						power.UsePowerOnOpponent(param, p.grid, p.grid.TheGrid);
					}
				}
			}

      if (isTutorial)
      {
        StageTracker.GetPowerUsed(true);
      }
    }

    private void OnScoreChanged(GridScript g, long score)
		{
			// Update UI
			GameUIScript.SetScore(g.player.index, score);
		}

		private void OnPowerChargeChanged(GridScript g, PowerType power, float charge, int direction)
		{
			// Update UI
			GameUIScript.SetPowerCharge(g.player.index, charge, direction);
		}

		private void OnLevelChanged(GridScript g, int l)
		{
			// Update UI
			GameUIScript.SetSpeed(g.player.index, l);
		}

		private void OnMultiplierChange(GridScript grid, int m)
		{
			// Update UI
			GameUIScript.SetMultiplier(grid.player.index, m);
		}

		private void Combo(GridScript g, ComboData c)
		{
			// Send blocks!
			if (players.Count > 1) GenerateGarbage(g, c);

			if (isTutorial)
			{
				StageTracker.GetCombo(g, c.blockCount);
			}
    }

    private void GenerateGarbage(GridScript g, ComboData c)
		{
			// Every action send garbages, but not always a lot of it.
			int width = 0;
			int count = 1;

			if (c.isChain == false)
			{
				// 3 blocs = 1x1 every 3 combos
				if (c.blockCount == 3 && c.multiplier % 3 == 0)
				{
					width = 1;
					count = 1;
				}
				// 4 blocs = 1x1
				else if (c.blockCount == 4)
				{
					width = 1;
					count = 1;
				}
				// 5 blocks
				else if (c.blockCount == 5)
				{
					width = 3;
					count = 1;
				} // L-shaped block = wow
				else if (c.blockCount > 5)
				{
					width = 5;
					count = 1;
				}
			}
			else
			{
				width = players[0].grid.settings.width;
			}

			// Factors
			if (settings.garbagesType == GarbagesType.None) return;
			if (settings.garbagesType == GarbagesType.Low)
			{
				if (width == 5)
				{
					count = 2;
				}

				width = 1;
			}

			if (width > 0)
			{
				var si = g.player.index;
				var sender = players.First(p => p.player.index == si);
				if (sender.lastPlayerTarget < 0) sender.lastPlayerTarget = sender.player.index;
				sender.lastPlayerTarget++;
				if (sender.lastPlayerTarget >= players.Count) sender.lastPlayerTarget = 0;

				var alivePlayers = players.Where(p => p.player.GameOver == false).ToArray();
				foreach (var p in alivePlayers)
				{
					int pi = p.player.index;
					if (
					  // Target the other player
					  (alivePlayers.Length <= 2 && pi != si)
					  ||
					  // Target rotation
					  (pi >= sender.lastPlayerTarget
					   && pi != si
					   && p.player.GameOver == false))
					{
						sender.lastPlayerTarget = pi;
						width = Mathf.Clamp(width, 1, settings.gridSettings.width);

						for (int i = 0; i < count; i++)
						{
							p.grid.AddGarbage(width, c.isChain, sender, c.comboLocation, c.definition.color);
						}

						break;
					}
				}
			}
		}

		private void GameOver(GridScript grid)
		{
			int gameOverPlayers = 0;

			foreach (var p in players)
			{
				if (p.player.index == grid.player.index)
				{
					p.player.GameOver = true;
					p.player.Score = grid.Score;
				}

				if (p.player.GameOver)
				{
					gameOverPlayers++;
				}
			}

			if (gameOverPlayers >= players.Count - 1)
			{
				foreach (var p in players)
				{
					if (p.player.GameOver == false)
					{
						p.grid.SetPause(true);
					}
				}

				// lose music ?
				musicSource.PlayOneShot(loseMusic);
				// set bool so that multiple sounds don't play
				lostByFillingScreen = true;
				// stop timer
				isOver = true;
				DOVirtual.DelayedCall(3f, TriggerGameOver);
			}
		}

		private void TriggerGameOver()
		{

			DOTweenGameObject.SetActive(false); //Deactivate DoTween GameObject when moving back to map
			Log.Warning("Game is ended.");
			SetPause(true);
			isOver = true;
			if (!isTutorial)
			{
				MapUIScript.mapInstance.wonLastGame = wonGame;
				// When the player wins, award them currency
				if (wonGame)
				{
					// Daily Bonus checks if its first time playing
					DailyBonusManager.Instance.AwardDailyBonus();

					// Award them the standard currency for winning
					CurrencyManager.Instance.AddCurrencyWithLimit(settings.currencyReward);
					if (currentLevelName != "")
					{
						int level = Int32.Parse(Regex.Match(currentLevelName, @"\d+").Value);
						if (level % 5 == 0)
						{
							//DialoguecurrentStage += 0.01f;
							GameManager.gameManager.endRegion = true;
						}
						level++;
						GameManager.gameManager.SaveLevel("Level " + level);
					}
				}

				// level ends, go back to map scene
				SceneManager.LoadSceneAsync("Map_t");
			}
			else
			{
				if (!wonGame)
				{
					StageTracker.ResetTutorial();
					SceneManager.LoadSceneAsync("Tutorial_Game");
				} else
				{
					StageTracker.SetTutorialStage(StageTracker.finalTutorialStage - 2f);
					SceneManager.LoadSceneAsync("Tutorial_Entry");
				}
			}
		}

#if UNITY_EDITOR
		public void SkipGame()
		{
			isOver = true;
			var pWinner = players[0];

			musicSource.PlayOneShot(winMusic);
			GameOverVersus(pWinner);
			wonGame = true;
		}



#endif
		#endregion

		#region Properties

		public GameSettings Settings => settings;

		public List<PlayerScript> Players => players;

		public Objective Objectives => objectives;

		public bool IsPaused => isPaused;

		public bool IsOver => isOver;

		#endregion
	}
}
