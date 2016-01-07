using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HoldemPlayerContract;

namespace RaiserBot
{
    public class RaiserBot : IHoldemPlayer
    {
        public void InitPlayer(int playerNum)
        {
        }

        public string Name
        {
            get
            {
                return "RaiserBot";
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
        }

        public void ReceiveHoleCards(Card hole1, Card hole2)
        {
        }

        public void SeeAction(eStage stage, int playerNum, eActionType action, int amount)
        {
        }

        public void GetAction(eStage stage, int callAmount, int minRaise, int maxRaise, int raisesRemaining, int potSize, out eActionType yourAction, out int amount)
        {
            yourAction = eActionType.ACTION_RAISE;
            amount = minRaise;
        }


        public void SeeBoardCard(eBoardCardType cardType, Card boardCard)
        {
        }

        public void SeePlayerHand(int playerNum, Card hole1, Card hole2, Hand bestHand)
        {
        }

        public void EndOfGame(int numPlayers, PlayerInfo[] players)
        {
        }

    }
}
