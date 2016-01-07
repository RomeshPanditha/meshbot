using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using HoldemPlayerContract;

namespace BetterBot
{
    public class BetterBot : IHoldemPlayer
    {
        private int _playerNum;
        private Card _hole1;
        private Card _hole2;
        private Card [] _board;
        private int _handNum = 0;

        public void InitPlayer(int playerNum)
        {
            // This is called once at the start of the game. playerNum is your unique identifer for the game
            _playerNum = playerNum;
            _board = new Card[5];
        }

        public string Name
        {
            // return the name of your player
            get
            {
                return "BetterBot";
            }
        }

        public bool IsObserver
        {
            get
            {
                return false;
            }
        }

        public void InitHand(int numPlayers, PlayerInfo[] players)
        {
            // this is called at the start of every hand and tells you the current status of all players (e.g. if is alive and stack size and who is dealer)
            // create a writer and open the file
            _handNum++;
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
            // receive your hole cards for this hand
            _hole1 = hole1;
            _hole2 = hole2;
        }

        public void SeeAction(eStage stage, int playerNum, eActionType action, int amount)
        {
            // this is called to inform you when any player (including yourself) makes an action (eg puts in blinds, checks, folds, calls, raises, or wins hand)
        }

        public void GetAction(eStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            amount = 0;
            yourAction = eActionType.ACTION_FOLD;

            // This is the bit where you need to put the AI (mostly likely based on info you receive in other methods)
            if (stage == eStage.STAGE_PREFLOP)
            {
                GetPreFlopAction(callAmount, minRaise, maxRaise, raisesRemaining, potSize, out yourAction, out amount);
            }
            else if (stage == eStage.STAGE_FLOP)
            {
                GetFlopAction(callAmount, minRaise, maxRaise, raisesRemaining, potSize, out yourAction, out amount);
            }
            else if (stage == eStage.STAGE_TURN)
            {
                GetTurnAction(callAmount, minRaise, maxRaise, raisesRemaining, potSize, out yourAction, out amount);
            }
            else if (stage == eStage.STAGE_RIVER)
            {
                GetRiverAction(callAmount, minRaise, maxRaise, raisesRemaining, potSize, out yourAction, out amount);
            }
            else if (stage == eStage.STAGE_SHOWDOWN)
            {
                GetShowdownAction(callAmount, minRaise, maxRaise, raisesRemaining, potSize, out yourAction, out amount);
            }
        }

        private void GetPreFlopAction(int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            bool bIsPair = false;
            bool bIsSuited = false;
            eRankType highRank;
            eRankType lowRank;
            int gap;

            amount = 0;
            yourAction = eActionType.ACTION_FOLD;

            if (_hole1.Rank == _hole2.Rank) 
            {
                bIsPair = true;
                lowRank = highRank = _hole1.Rank;
            }
            else if (_hole1.Rank > _hole2.Rank)
            {
                highRank = _hole1.Rank;
                lowRank = _hole2.Rank;
            }
            else
            {
                highRank = _hole2.Rank;
                lowRank = _hole1.Rank;
            }

            gap = highRank - lowRank;
    
            if(_hole1.Suit == _hole2.Suit)
            {
                bIsSuited = true;
            }

            if (bIsPair)
            {
                if (highRank >= eRankType.RANK_EIGHT)
                {
                    yourAction = eActionType.ACTION_RAISE;
                    amount = minRaise;
                }
                else if (highRank >= eRankType.RANK_FIVE)
                {
                    yourAction = eActionType.ACTION_CALL;
                    amount = callAmount;
                }
            }
            else
            {
                if (highRank >= eRankType.RANK_KING && lowRank >= eRankType.RANK_EIGHT)
                {
                    yourAction = eActionType.ACTION_RAISE;
                    amount = minRaise;
                }
                else if (highRank >= eRankType.RANK_JACK)
                {
                    yourAction = eActionType.ACTION_CALL;
                    amount = callAmount;
                }
                else if (bIsSuited && gap == 1)
                {
                    yourAction = eActionType.ACTION_CALL;
                    amount = callAmount;
                }
            }
        }

        private void GetFlopAction(int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            yourAction = eActionType.ACTION_RAISE;
            amount = callAmount;
        }

        private void GetTurnAction(int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            yourAction = eActionType.ACTION_RAISE;
            amount = callAmount;
        }

        private void GetRiverAction(int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            yourAction = eActionType.ACTION_RAISE;
            amount = callAmount;
        }

        private void GetShowdownAction(int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            // if stage is the showdown then choose whether to show your hand or fold
            yourAction = eActionType.ACTION_SHOW;
            amount = 0;
        }

        public void SeeBoardCard(eBoardCardType cardType, Card boardCard)
        {
            // this is called to inform you of the board cards (3 flop cards, turn and river)
            _board[(int)cardType] = boardCard;
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
            // this is called to inform you of another players hand during the show down. 
            // bestHand is the best hand that they can form with their hole cards and the five board cards
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
        }
    }
}
