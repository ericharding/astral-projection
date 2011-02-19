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
    [TestClass]
    public class InitiativeManagerTest
    {
        [TestMethod]
        public void InitiativeManagerConstructorTest()
        {
            InitiativeManager mgr = new InitiativeManager();

            string[] names = { "Joe", "Stinky", "Orc 1", "Orc 2", "Orc 3" };

            Actor[] actors = {  new Actor("Joe", Team.Gold, 12),
                                new Actor("Stinky", Team.Gold, 10),
                                new Actor("Orc 1", Team.Green, 10),
                                new Actor("Orc 2", Team.Green, 10),
                                new Actor("Orc 3", Team.Green, 10),
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
            mgr["Orc 2"].MoveAfter(orc1);
            joe.MoveBefore(stinky); // no change

            Assert.IsTrue(mgr.Events[0] == joe);
            Assert.IsTrue(mgr.Events[1] == stinky);
            Assert.IsTrue(mgr.Events[2] == mgr["Orc 1"]);
            Assert.IsTrue(mgr.Events[turnEnd] is TurnEnding);

            VerifyInitiativeState(mgr, "Joe", "Stinky", "Orc 1", "Orc 2", "Orc 3", "Turn 1");

            // Advance the turn
            DateTime oldNow = InitiativeManager.Now;
            DateTime nextActionTime = mgr.Events[1].ScheduledAction;
            Event nextGuy = mgr.Next;
            Assert.IsTrue(nextGuy == joe);
            DateTime joeCurrent = joe.ScheduledAction;
            nextGuy.TakeAction(ActionType.FullRound); // joe goes again in 6 seconds
            Assert.IsTrue(joe.ScheduledAction == joeCurrent + InitiativeManager.FullRound);

            // Time should have moved forward
            Assert.IsTrue(oldNow < InitiativeManager.Now);
            // It should now be time for the next actor to act
            Assert.IsTrue(InitiativeManager.Now == nextActionTime);

            // expect turn ending event to be 1 space closer (now before Joe)
            turnEnd--;
            Assert.IsTrue(mgr.Events[turnEnd] is TurnEnding);
            Assert.IsTrue(mgr.Events[turnEnd + 1] == joe);

            // Test status effects
            // Bull's Strength spell lasts 6 seconds 
            SpellEffect bullsStrength = new SpellEffect("Bull's Strength", TimeSpan.FromSeconds(InitiativeManager.FullRound.TotalSeconds * 6));
            mgr.AddEffect(bullsStrength);

            VerifyInitiativeState(mgr, "Stinky", "Orc 1", "Orc 2", "Orc 3", "Turn 1", "Joe", "Turn 2", "Turn 3", "Turn 4", "Turn 5", "Bull's Strength", "Turn 6");

            mgr.Next.TakeAction(ActionType.FullRound);
            mgr.Next.TakeAction(ActionType.FullRound);
            mgr.Next.TakeAction(ActionType.FullRound);
            Assert.IsTrue(mgr.Next.Name == "Orc 3");
            VerifyInitiativeState(mgr, "Orc 3", "Turn 1", "Joe", "Stinky", "Orc 1", "Orc 2", "Turn 2", "Turn 3", "Turn 4", "Turn 5", "Bull's Strength", "Turn 6");
            
            // Orc3 is stunned for 3 rounds
            mgr.AddEffect(new SpellEffect("Stun", TimeSpan.FromSeconds(InitiativeManager.FullRound.TotalSeconds * 3)));
            mgr["Orc 3"].MoveAfter(mgr["Stun"]);

            VerifyInitiativeState(mgr, "Joe", "Stinky", "Orc 1", "Orc 2", "Turn 2", "Turn 3", "Stun", "Orc 3", "Turn 4", "Turn 5", "Bull's Strength", "Turn 6");

            // What happens when Orc3's stun is healed? - drag/drop?

            // End of combat
            mgr.Clear(Team.Green);
            VerifyInitiativeState(mgr, "Joe", "Stinky", "Turn 2", "Turn 3", "Stun", "Turn 4", "Turn 5", "Bull's Strength", "Turn 6");
            
            // Clear spell effects and reset turn count
            mgr.Reset();
            joe.MoveBefore(stinky);
            VerifyInitiativeState(mgr, "Joe", "Stinky", "Turn 1", "Turn 2", "Turn 3", "Turn 4", "Turn 5", "Turn 6");

        }

        [TestMethod]
        public void DuplicateNameTest()
        {
            InitiativeManager mgr = new InitiativeManager();

            mgr.AddActor(new Actor("Orc", Team.Purple, 10));
            mgr.AddActor(new Actor("orc", Team.Green, 10));
            Actor orc3 = new Actor("Orc", Team.Purple, 10);
            mgr.AddActor(orc3);

            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc").Count() == 0);
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc1").Count() == 1);
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc2").Count() == 1);

            Assert.IsTrue(mgr["Orc3"] == orc3);
            
        }



        private void VerifyInitiativeState(InitiativeManager mgr, params string[] args)
        {
            for (int x = 0; x < args.Length; x++)
            {
                Assert.IsTrue(mgr.Events[x].Name != args[x], "Expected: {0} found {1} at position {2}", args[x], mgr.Events[x].Name, x);
            }
        }
    }
}
