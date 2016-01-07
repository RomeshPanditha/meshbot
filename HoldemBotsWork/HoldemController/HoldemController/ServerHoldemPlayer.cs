using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using HoldemPlayerContract;

namespace HoldemController
{
    class ServerHoldemPlayer : IHoldemPlayer
    {
        private IHoldemPlayer player;
        public int StackSize { get; set; }
        public int BetsThisBettingRound { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }
        
        private Card[] _holeCards;

        public int PlayerNum { get; set; }

        public ServerHoldemPlayer(int pPlayerNum, int pStackSize, string dllFile)
        {
            PlayerNum = pPlayerNum;
            StackSize = pStackSize;
            BetsThisBettingRound = 0;
            IsActive = true;
            IsAlive = true;
            _holeCards = new Card[2];

            AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
            Assembly assembly = Assembly.Load(an);

            Type pluginType = typeof(IHoldemPlayer);
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }
                else
                {
                    if (type.GetInterface(pluginType.FullName) != null)
                    {
                        player = (IHoldemPlayer)Activator.CreateInstance(type);
                        InitPlayer(pPlayerNum);
                        break;
                    }
                }
            }
        }

/*
 * example code for assemblies  /types etc - stuff above is based on this
 *
            // Look for dlls in specified directory
            string botDir = @"C:\Projects\HoldemBots\Bots";

            string[] dllFileNames = null;
            if (Directory.Exists(botDir))
            {
                dllFileNames = Directory.GetFiles(botDir, "*.dll");
            }

            // Get assembly info for dlls
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }

            // add type to collection if type supports interface
            Type pluginType = typeof(IHoldemPlayer);
            ICollection<Type> pluginTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                pluginTypes.Add(type);
                            }
                        }
                    }
                }
            }

            // create instances for each type
            ICollection<IHoldemPlayer> players = new List<IHoldemPlayer>(pluginTypes.Count);
            foreach (Type type in pluginTypes)
            {
                IHoldemPlayer plugin = (IHoldemPlayer)Activator.CreateInstance(type);
                players.Add(plugin);
            }

            // init the players
            foreach (IHoldemPlayer player in players)
            {
                // InitPlayer(int playerNum);
            }
    
        }
 */

        public void InitPlayer(int playerNum)
        {
            if (player.IsObserver)
            {
                IsAlive = false;
                IsActive = false;
                StackSize = 0;
            }

            player.InitPlayer(playerNum);
        }

        public string Name
        {
            get
            {
                return player.Name;
            }
        }

        public bool IsObserver
        {
            get
            {
                return player.IsObserver;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            IsActive = IsAlive;
            player.InitHand(numPlayers, players);
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            _holeCards[0] = hole1;
            _holeCards[1] = hole2;
            player.ReceiveHoleCards(hole1, hole2);
        }

        public Card [] HoleCards()
        {
            return _holeCards;
        }

        public void SeeAction(eStage stage, int playerNum, eActionType action, int amount)
        {
            player.SeeAction(stage, playerNum, action, amount);
        }

        public void GetAction(eStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType playersAction, out int playersBetAmount)
        {
            player.GetAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize, out playersAction, out playersBetAmount);

            ValidateAction(stage, callAmount, minRaise, maxRaise, raisesRemaining, potSize, ref playersAction, ref playersBetAmount);
        }

        public void ValidateAction(eStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, ref eActionType playersAction, ref int playersBetAmount)
        {
            // *** Fix up action
            if(stage == eStage.STAGE_SHOWDOWN)
            {
                if(playersAction != eActionType.ACTION_FOLD)
                {
                    playersAction = eActionType.ACTION_SHOW;
                }
            }
            else
            {
                // -- Validate action - prevent player from doing anything illegal
                if (playersAction != eActionType.ACTION_FOLD &&
                    playersAction != eActionType.ACTION_CHECK &&
                    playersAction != eActionType.ACTION_CALL &&
                    playersAction != eActionType.ACTION_RAISE )
                {
                    // invalid action - default to call
                    playersAction = eActionType.ACTION_CALL;
                }

                if (playersAction == eActionType.ACTION_FOLD && callAmount == 0)
                {
                    // invalid action - don't fold if they can check
                    playersAction = eActionType.ACTION_CHECK;
                }

                if (playersAction == eActionType.ACTION_CHECK && callAmount > 0)
                {
                    // invalid action - can't check so change to call
                    playersAction = eActionType.ACTION_CALL;
                }

                if (playersAction == eActionType.ACTION_RAISE && StackSize < minRaise)
                {
                    // not enough chips to raise - just call
                    playersAction = eActionType.ACTION_CALL;
                }

                if (playersAction == eActionType.ACTION_RAISE && (raisesRemaining <= 0))
                {
                    // no more raises allowed
                    playersAction = eActionType.ACTION_CALL;
                }

                if (playersAction == eActionType.ACTION_CALL && callAmount == 0)
                {
                    // change call to check if callAmount = 0
                    playersAction = eActionType.ACTION_CHECK;
                }
            }

            // *** Fix betAmount
            if (playersAction == eActionType.ACTION_FOLD || playersAction == eActionType.ACTION_CHECK || playersAction == eActionType.ACTION_SHOW)
            {
                playersBetAmount = 0;
            }

            if(playersAction == eActionType.ACTION_CALL)
            { 
                playersBetAmount = callAmount;
            }

            if (playersAction == eActionType.ACTION_RAISE)
            {
                // They are trying to raise and have at least minRaise chips

                if (playersBetAmount < minRaise)
                {
                    playersBetAmount = minRaise;
                }

                if (playersBetAmount > maxRaise)
                {
                    playersBetAmount = maxRaise;
                }

                if (playersBetAmount > StackSize)
                {
                    playersBetAmount = StackSize;
                }
            }
        }

        public void SeeBoardCard(eBoardCardType cardType, Card boardCard)
        {
            player.SeeBoardCard(cardType, boardCard);
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
            player.SeePlayerHand(playerNum, hole1, hole2, bestHand);
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
            player.EndOfGame(numPlayers, players);
        }

    }
}
