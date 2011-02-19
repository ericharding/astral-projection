using Astral.Projector.Initiative;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AstralTest
{
    [TestClass()]
    public class EventFactoryTest
    {
        [TestMethod]
        public void EventCreateScript()
        {
            Actor complex = (Actor)EventFactory.Create("Some ugly orc ac: 12  att:2d6 polearm of spikyness ac:22 limbs:4 tusks:yellow and curved foo:bar hp:3d8+2");

            Actor orc = (Actor)EventFactory.Create("Orc 10");
            Actor joe = (Actor)EventFactory.Create("Joe of the wastes 3d8");
            Actor Fred = (Actor)EventFactory.Create("Fred hp:2d6 ac:12 att:2d6x2 ab:3 team:RedFlag");
            Actor bod = (Actor)EventFactory.Create("The Bodak hd:9d12 init:+6 ac:20 team:3");
            Event bull = (SpellEffect)EventFactory.Create("Bull's Strength dur:10");
            SpellEffect cat = (SpellEffect)EventFactory.Create("Cat's Super Feline Awesomeness dur:12");
            Event bianc = (SpellEffect)EventFactory.Create("Bianca's Sneezing Window dur:9");
            
        }

        [TestMethod]
        public void TestRoll()
        {
            for (int x = 0; x < 100; x++)
            {
                int result = EventFactory.Roll("3d6+2");
                Assert.IsTrue(result >= 5 && result <= 20);
            }
            for (int x = 0; x < 100; x++)
            {
                int result = EventFactory.Roll("1d10");
                Assert.IsTrue(result >= 1 && result <= 10);
            }
            for (int x = 0; x < 100; x++)
            {
                int result = EventFactory.Roll("3d8");
                Assert.IsTrue(result >= 3 && result <= 24);
            }

            Assert.IsTrue(10 == EventFactory.Roll("10"));
        }
    }
}
