using Astral.Projector.Initiative;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AstralTest
{


    /// <summary>
    ///This is a test class for InitiativeManagerTest and is intended
    ///to contain all InitiativeManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InitiativeManagerTest
    {
        [Flags]
        enum FooBar { foo, bar }

        [TestMethod()]
        public void InitiativeManagerConstructorTest()
        {
            InitiativeManager mgr = new InitiativeManager();

            string[] names = { "Joe", "Stinky", "Orc 1", "Orc 2", "Orc 3" };

            Tuple<string, bool>[] nameFlags = names.Select(s => new Tuple<string, bool>(s, false)).ToArray();

            Actor[] actors = {  new Actor("Joe", Team.Gold),
                                new Actor("Stinky", Team.Gold),
                                new Actor("Orc 1", Team.Green),
                                new Actor("Orc 2", Team.Green),
                                new Actor("Orc 3", Team.Green),
                             };
            Actor joe = actors[0];
            Actor stinky = actors[1];
            Actor orc1 = actors[3];

            foreach (var actor in actors)
            {
                mgr.AddActor(actor);
            }

            Assert.IsTrue(joe == mgr["Joe"]);
            Assert.IsTrue(orc1 == mgr["Orc 1"]);

            // Actors are placed in the first turn in order.
            for (int x = 0; x < actors.Length - 1; x++)
            {
                Assert.IsTrue(actors[x].ScheduledAction < actors[x + 1].ScheduledAction);
                Assert.IsTrue(actors[x].ScheduledAction - InitiativeManager.Now > InitiativeManager.FullRound);
            }

            int turnEnd = 0;
            foreach (Event evnt in mgr.Events)
            {
                turnEnd++;
                if (evnt is TurnEnding)
                {
                    Assert.IsTrue(evnt.ScheduledAction == InitiativeManager.Now + InitiativeManager.FullRound);
                    break;
                }
            }

            // Randomize order
            mgr.Shuffle();

            // Joe goes first
            mgr.MoveBefore(mgr["Joe"], mgr.Events[0]);
            mgr.MoveAfter(stinky, mgr["joe"]);
            mgr.MoveAfter(orc1, stinky);

            Assert.IsTrue(mgr.Events[0] == joe);
            Assert.IsTrue(mgr.Events[1] == stinky);
            Assert.IsTrue(mgr.Events[2] == mgr["Orc 1"]);
            Assert.IsTrue(mgr.Events[turnEnd] is TurnEnding);

            // Advance the turn
            Event nextGuy = mgr.Next;
            Assert.IsTrue(nextGuy == joe);
            DateTime joeCurrent = joe.ScheduledAction;
            nextGuy.TakeAction(ActionType.FullRound); // joe goes again in 6 seconds
            Assert.IsTrue(joe.ScheduledAction == joeCurrent + InitiativeManager.FullRound);

            // expect turn ending to move forward
            turnEnd--;
            Assert.IsTrue(mgr.Events[turnEnd] is TurnEnding);
            Assert.IsTrue(mgr.Events[turnEnd + 1] == joe);

        }
    }
}
