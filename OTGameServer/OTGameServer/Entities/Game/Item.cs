using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TGame.Entities
{
    public enum EquipmentSlot
    {
        None = 0,
        Weapon
    }

    public abstract class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        public abstract bool IsStackable { get; }
    }

    public class Equipment : Item
    {
        public override bool IsStackable => false;

        public EquipmentSlot Slot { get; set; }
        //public int MaxEquipsCount { get; set; }
        public Stats Stats { get; set; }

        public Equipment() { }
        public Equipment(JToken data)
        {
            Name = (string)data["Name"];
            Price = (int)data["Price"];
            Slot = (EquipmentSlot)(int)data["Price"];
            Stats = data["Stats"].ToObject<Stats>();
        }
    }
}
