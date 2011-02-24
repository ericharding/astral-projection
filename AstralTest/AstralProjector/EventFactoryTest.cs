using Astral.Projector.Initiative;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AstralTest
{
    [TestClass()]
    public class EventFactoryTest
    {
        [TestMethod]
        public void EventCreateScript()
        {
            InitiativeManager mgr = new InitiativeManager();

            Actor complex = (Actor)mgr.CreateEvent("Some ugly orc ac: 12  att:2d6 polearm of spikyness ac:22 limbs:4 tusks:yellow and curved foo:bar hp:3d8+2");
            Assert.IsTrue(complex.Name == "Some ugly orc");
            Assert.IsTrue(complex.CurrentHealth >= 5 && complex.CurrentHealth <= 26);
            Assert.IsTrue(complex.Team == mgr.CurrentTeam);
            Assert.IsTrue(complex.Properties["limbs"] == "4");

            Actor orc = (Actor)mgr.CreateEvent("Orc 10");
            Assert.IsTrue(orc.Name == "Orc");
            Assert.IsTrue(orc.MaxHealth == 10);

            Actor joe = (Actor)mgr.CreateEvent("Joe of the wastes 3d8");
            Assert.IsTrue(joe.Name == "Joe of the wastes");
            Assert.IsTrue(joe.CurrentHealth >= 3);

            Actor Fred = (Actor)mgr.CreateEvent("Fred hp:2d6 ac:12 att:2d6x2 ab:3 team:RedFlag");
            Assert.IsTrue(Fred.Name == "Fred");
            Assert.IsTrue(Fred.CurrentHealth >= 2 && Fred.CurrentHealth <= 12);
            Assert.IsTrue(Fred.Team == Team.RedFlag);

            Actor bod = (Actor)mgr.CreateEvent("The Bodak hd:9d12 init:+6 ac:20 team:3");
            Assert.IsTrue(bod.Name == "The Bodak");
            Assert.IsTrue(bod.MaxHealth >= 9 && bod.MaxHealth <= 108);
            Assert.IsTrue(bod.Team == (Team)3);
            Assert.IsTrue(bod.Properties["init"] == "+6");
            Assert.IsTrue(bod.Properties["ac"] == "20");

            Event bull = (SpellEffect)mgr.CreateEvent("Bull's Strength dur:10");
            Assert.IsTrue(bull.Name == "Bull's Strength");

            SpellEffect cat = (SpellEffect)mgr.CreateEvent("Cat's Super Feline Awesomeness dur:12");
            Assert.IsTrue(cat.Name == "Cat's Super Feline Awesomeness");
            Assert.IsTrue(cat.ScheduledAction == InitiativeManager.Now + TimeSpan.FromSeconds(6 * 12));

            Event bianc = (SpellEffect)mgr.CreateEvent("Bianca's Sneezing Window dur:9");
            Assert.IsTrue(bianc.Name == "Bianca's Sneezing Window");
        }

        [TestMethod]
        public void TestRoll()
        {
            for (int x = 0; x < 100; x++)
            {
                int result = InitiativeManager.Roll("3d6+2");
                Assert.IsTrue(result >= 5 && result <= 20);
            }
            for (int x = 0; x < 100; x++)
            {
                int result = InitiativeManager.Roll("1d10");
                Assert.IsTrue(result >= 1 && result <= 10);
            }
            for (int x = 0; x < 100; x++)
            {
                int result = InitiativeManager.Roll("3d8");
                Assert.IsTrue(result >= 3 && result <= 24);
            }

            Assert.IsTrue(10 == InitiativeManager.Roll("10"));
        }
    }
}
