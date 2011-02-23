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

            Actor[] actors = {  new Actor("Joe", Team.Gold, 12, mgr),
                                new Actor("Stinky", Team.Gold, 10, mgr),
                                new Actor("Orc 1", Team.Green, 10, mgr),
                                new Actor("Orc 2", Team.Green, 10, mgr),
                                new Actor("Orc 3", Team.Green, 10, mgr),
                             };
            Actor joe = actors[0];
            Actor stinky = actors[1];
            Actor orc1 = actors[2];

            foreach (var actor in actors)
            {
                mgr.AddEvent(actor);
            }

            Assert.IsTrue(joe == mgr["Joe"]);
            Assert.IsTrue(orc1 == mgr["Orc 1"]);

            // All units are in the manager in the first turn and in random order
            for (int x = 0; x < actors.Length - 1; x++)
            {
                Assert.IsTrue(actors[x].ScheduledAction - InitiativeManager.Now < InitiativeManager.FullRound);
            }

            int turnEnd = 0;
            foreach (Event evnt in mgr.Events)
            {
                if (evnt is TurnEnding)
                {
                    Assert.IsTrue(evnt.ScheduledAction == InitiativeManager.Now + InitiativeManager.FullRound);
                    break;
                }
                turnEnd++;
            }

            // Randomize order
            mgr.Shuffle();

            DateTime rightNow = InitiativeManager.Now;

            // Joe goes first
            mgr.MoveBefore(mgr["Joe"], mgr.Events[0]);
            mgr.MoveAfter(stinky, mgr["joe"]);
            mgr.MoveAfter(orc1, stinky);
            mgr["Orc 2"].MoveAfter(orc1);
            joe.MoveBefore(stinky); // no change

            Assert.IsTrue(rightNow <= InitiativeManager.Now, "Time is not allowed to move if nobody is acting");

            Assert.IsTrue(mgr.Events[0] == joe);
            Assert.IsTrue(mgr.Events[1] == stinky);
            Assert.IsTrue(mgr.Events[2] == mgr["Orc 1"]);
            Assert.IsTrue(mgr.Events[turnEnd] is TurnEnding);

            VerifyInitiativeState(mgr, "Joe", "Stinky", "Orc 1", "Orc 2", "Orc 3", "Turn 2");

            // Make sure nobody went back in time!
            Assert.IsTrue((mgr["Turn 2"].ScheduledAction - mgr["Joe"].ScheduledAction).TotalSeconds <= 6);

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
            SpellEffect bullsStrength = new SpellEffect("Bull's Strength", TimeSpan.FromSeconds(InitiativeManager.FullRound.TotalSeconds * 6), mgr);
            mgr.AddEvent(bullsStrength);

            VerifyInitiativeState(mgr, "Stinky", "Orc 1", "Orc 2", "Orc 3", "Turn 2", "Joe", "Turn 3", "Turn 4", "Turn 5", "Turn 6", "Turn 7", "Bull's Strength");

            mgr.Next.TakeAction(ActionType.FullRound);
            mgr.Next.TakeAction(ActionType.FullRound);
            mgr.Next.Complete(); // For an Actor this is equivalent to: mgr.Next.TakeAction(ActionType.FullRound);
            Assert.IsTrue(mgr.Next.Name == "Orc 3");
            VerifyInitiativeState(mgr, "Orc 3", "Turn 2", "Joe", "Stinky", "Orc 1", "Orc 2", "Turn 3", "Turn 4", "Turn 5", "Turn 6", "Turn 7", "Bull's Strength");
            
            // Orc3 is stunned for 3 rounds
            mgr.AddEvent(new SpellEffect("Stun", TimeSpan.FromSeconds(InitiativeManager.FullRound.TotalSeconds * 3), mgr));
            mgr["Orc 3"].MoveAfter(mgr["Stun"]);

            TurnEnding turn2 = (TurnEnding)mgr.Next;
            turn2.Complete();

            VerifyInitiativeState(mgr, "Joe", "Stinky", "Orc 1", "Orc 2", "Turn 3", "Turn 4", "Stun", "Orc 3", "Turn 5", "Turn 6", "Turn 7", "Bull's Strength", "Turn 8");

            // What happens when Orc3's stun is healed? - drag/drop?

            // End of combat
            mgr.Clear(Team.Green);
            VerifyInitiativeState(mgr, "Joe", "Stinky", "Turn 3", "Turn 4", "Stun", "Turn 5", "Turn 6", "Turn 7", "Bull's Strength", "Turn 8");
            
            // Clear spell effects and reset turn count
            mgr.Reset();
            joe.MoveBefore(stinky);
            VerifyInitiativeState(mgr, "Joe", "Stinky", "Turn 2", "Turn 3", "Turn 4", "Turn 5", "Turn 6", "Turn 7");

        }

        [TestMethod]
        public void DuplicateNameTest()
        {
            InitiativeManager mgr = new InitiativeManager();

            mgr.AddEvent(new Actor("Orc#", Team.Purple, 10, mgr));
            mgr.AddEvent(new Actor("Orc#", Team.Green, 10, mgr));
            Event orc3 = mgr.CreateEvent("Orc# hp:10 team:4");
            Assert.IsTrue(orc3.Name == "Orc#");
            mgr.AddEvent(orc3);

            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc").Count() == 0);
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc1").Count() == 1);
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc2").Count() == 1);

            Assert.IsTrue(mgr["Orc3"] == orc3);
            Assert.IsTrue(orc3.Name == "Orc3");

            mgr.AddEvent(mgr.CreateEvent("Orc # 10"));
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc 1").Count() == 1);
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc1").Count() == 1);

            mgr.AddEvent(new Actor("Orc1", Team.YellowFlag, 10, mgr));
            Assert.IsTrue(mgr.Events.Where(e => e.Name == "Orc1").Count() == 2);
        }

        [TestMethod]
        public void TurnCycleTest()
        {
            const int turns_to_show = 8;
            List<string> turns = new List<string>(turns_to_show);
            int nextTurn = 2;
            for (; nextTurn < turns_to_show+2; nextTurn++)
            {
                turns.Add("Turn " + nextTurn);
            }

            InitiativeManager mgr = new InitiativeManager();

            VerifyInitiativeState(mgr, turns.ToArray());

            Action Advance = () =>
            {
                turns.RemoveAt(0);
                turns.Add("Turn " + nextTurn);
                nextTurn++;
            };

            mgr.Next.Complete();
            Advance();
            VerifyInitiativeState(mgr, turns.ToArray());

            for (int x = 0; x < 100; x++)
            {
                mgr.Next.Complete();
                Advance();
                VerifyInitiativeState(mgr, turns.ToArray());
            }
        }

        [TestMethod]
        public void UndoTest()
        {
            InitiativeManager mgr = new InitiativeManager();
            mgr.AddEvent("Goblin hp:10 ac:12");
            mgr.AddEvent("Mr. Lucky # 1");
            mgr.AddEvent("Mr. Lucky # 1");
            mgr["Goblin"].MoveBefore(mgr.Events[0]);
            mgr["Mr. Lucky 1"].MoveAfter(mgr["Goblin"]);
            VerifyInitiativeState(mgr, "Goblin", "Mr. Lucky 1", "Mr. Lucky 2", "Turn 2");
            mgr.Next.TakeAction(ActionType.Standard); //+4s
            mgr.Next.TakeAction(ActionType.FullRound); //+6s
            VerifyInitiativeState(mgr, "Mr. Lucky 2", "Goblin", "Turn 2", "Mr. Lucky 1");
            mgr.Undo();
            VerifyInitiativeState(mgr, "Mr. Lucky 1", "Mr. Lucky 2", "Goblin", "Turn 2");
            mgr.Next.TakeAction(ActionType.FullRound);
            VerifyInitiativeState(mgr, "Mr. Lucky 2", "Goblin", "Turn 2", "Mr. Lucky 1");
            mgr.Next.TakeAction(ActionType.Standard); // luck2
            VerifyInitiativeState(mgr, "Goblin", "Mr. Lucky 2", "Turn 2", "Mr. Lucky 1");
            mgr.Undo();
            mgr.Next.TakeAction(ActionType.FullRound);
            VerifyInitiativeState(mgr, "Goblin", "Turn 2", "Mr. Lucky 1", "Mr. Lucky 2");
        }

        [TestMethod]
        public void TurnEnding()
        {
            InitiativeManager mgr = new InitiativeManager();
            mgr.AddEvent("Goblin hp:1d8+2 ac:12");
            mgr.AddEvent("Orc hp:3d8 ac:13");
            mgr["Goblin"].MoveBefore(mgr["Orc"]);
            Actor gob = (Actor)mgr.Events[0];
            Actor orc = (Actor)mgr.Events[1];
            Assert.IsTrue(gob.HasAttackOfOpportunity);
            gob.HasAttackOfOpportunity = false;
            gob.TakeAction(ActionType.Standard);
            Assert.IsTrue(gob.HasAttackOfOpportunity == false);
            orc.HasAttackOfOpportunity = false;
            orc.TakeAction(ActionType.FullRound); // Orc will have an attack on his next turn
            Assert.IsTrue(mgr.Next == gob);
            Assert.IsTrue(orc.HasAttackOfOpportunity == false); // not yet turn hasn't ended
            gob.TakeAction(ActionType.Standard);
            Assert.IsTrue(mgr.Next is TurnEnding);
            mgr.Next.Complete();
            Assert.IsTrue(orc.HasAttackOfOpportunity == true);
            Assert.IsTrue(gob.HasAttackOfOpportunity == true); // not goblins turn yet but he should have an attack b/c he took 2x 4 second actions
            VerifyInitiativeState(mgr, "Orc", "Goblin", "Turn 3");
            
        }

        private void VerifyInitiativeState(InitiativeManager mgr, params string[] args)
        {
            for (int x = 0; x < args.Length; x++)
            {
                Assert.IsTrue(mgr.Events[x].Name == args[x], "Expected: {0} found {1} at position {2}", args[x], mgr.Events[x].Name, x);
            }
        }
    }
}
