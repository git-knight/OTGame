using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TGame.Entities
{
    public class ItemStack
    {
        public int Id { get; set; }

        public int TypeId { get; set; }
        [Required] public virtual Hero Owner { get; set; }
        public int Amount { get; set; }

        [NotMapped] public Item Type { get => GameHub.Items[TypeId]; set { TypeId = value.Id; } }

        [NotMapped] public bool IsEquipped { get { return Amount == -1; } set { Amount = value ? -1 : 1; } }

        internal object ToPlayer()
        {
            return new
            {
                Id,
                Type.Name,
                canBeEquipped = Type is Equipment,
                slot = (Type as Equipment)?.Slot ?? EquipmentSlot.None,
                amount = IsEquipped ? 0 : Amount,
                IsEquipped
            };
        }
    }
}
