﻿using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Zone.Obelisks;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private void SendDetails() => _packetsHelper.SendDetails(Client, this);

        protected override void SendCurrentHitpoints() => _packetsHelper.SendCurrentHitpoints(Client, this);

        private void SendInventoryItems() => _packetsHelper.SendInventoryItems(Client, InventoryItems.Values.ToList());

        private void SendLearnedSkills() => _packetsHelper.SendLearnedSkills(Client, this);

        private void SendOpenQuests() => _packetsHelper.SendQuests(Client, Quests.Where(q => !q.IsFinished));

        private void SendFinishedQuests() => _packetsHelper.SendFinishedQuests(Client, Quests.Where(q => q.IsFinished));

        private void SendQuestStarted(Quest quest, int npcId = 0) => _packetsHelper.SendQuestStarted(Client, quest.Id, npcId);

        private void SendQuestFinished(Quest quest, int npcId = 0) => _packetsHelper.SendQuestFinished(Client, quest, npcId);

        private void SendFriendRequest(Character requester) => _packetsHelper.SendFriendRequest(Client, requester);

        private void SendFriendOnline(int friendId, bool isOnline) => _packetsHelper.SendFriendOnline(Client, friendId, isOnline);

        private void SendFriends() => _packetsHelper.SendFriends(Client, Friends.Values);

        private void SendFriendAdd(Character friend) => _packetsHelper.SendFriendAdded(Client, friend);

        private void SendFriendResponse(bool accepted) => _packetsHelper.SendFriendResponse(Client, accepted);

        private void SendFriendDelete(int id) => _packetsHelper.SendFriendDelete(Client, id);

        private void SendQuestCountUpdate(ushort questId, byte index, byte count) => _packetsHelper.SendQuestCountUpdate(Client, questId, index, count);

        private void SendActiveBuffs() => _packetsHelper.SendActiveBuffs(Client, ActiveBuffs);

        private void SendAddBuff(ActiveBuff buff) => _packetsHelper.SendAddBuff(Client, buff);

        private void SendRemoveBuff(ActiveBuff buff) => _packetsHelper.SendRemoveBuff(Client, buff);

        private void SendSkillBar() => _packetsHelper.SendSkillBar(Client, QuickItems);

        protected override void SendAdditionalStats()
        {
            if (Client != null) _packetsHelper.SendAdditionalStats(Client, this);
        }

        private void SendMaxHP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.HP);

        private void SendMaxSP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.SP);

        private void SendMaxMP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.MP);

        private void SendAttackStart() => _packetsHelper.SendAttackStart(Client);

        private void SendAutoAttackWrongTarget(IKillable target) => _packetsHelper.SendAutoAttackWrongTarget(Client, this, target);

        private void SendAutoAttackCanNotAttack(IKillable target) => _packetsHelper.SendAutoAttackCanNotAttack(Client, this, target);

        private void SendSkillAttackCanNotAttack(IKillable target, Skill skill) => _packetsHelper.SendSkillAttackCanNotAttack(Client, this, skill, target);

        private void SendSkillWrongTarget(IKillable target, Skill skill) => _packetsHelper.SendSkillWrongTarget(Client, this, skill, target);

        private void SendSkillWrongEquipment(IKillable target, Skill skill) => _packetsHelper.SendSkillWrongEquipment(Client, this, target, skill);

        private void SendNotEnoughMPSP(IKillable target, Skill skill) => _packetsHelper.SendNotEnoughMPSP(Client, this, target, skill);

        private void SendUseSMMP(ushort needMP, ushort needSP) => _packetsHelper.SendUseSMMP(Client, needMP, needSP);

        private void SendCooldownNotOver(IKillable target, Skill skill) => _packetsHelper.SendCooldownNotOver(Client, this, target, skill);

        protected override void SendMoveAndAttackSpeed()
        {
            if (Client != null) _packetsHelper.SendMoveAndAttackSpeed(Client, this);
        }

        private void SendRunMode() => _packetsHelper.SendRunMode(Client, this);

        private void SendTargetAddBuff(IKillable target, ActiveBuff buff) => _packetsHelper.SendTargetAddBuff(Client, target, buff);

        private void SendTargetRemoveBuff(IKillable target, ActiveBuff buff) => _packetsHelper.SendTargetRemoveBuff(Client, target, buff);

        public void SendAddItemToInventory(Item item) => _packetsHelper.SendAddItem(Client, item);

        public void SendRemoveItemFromInventory(Item item, bool fullRemove) => _packetsHelper.SendRemoveItem(Client, item, fullRemove);

        public void SendWeather() => _packetsHelper.SendWeather(Client, Map);

        public void SendObelisks() => _packetsHelper.SendObelisks(Client, Map.Obelisks.Values);

        public void SendObeliskBroken(Obelisk obelisk) => _packetsHelper.SendObeliskBroken(Client, obelisk);

        public void SendCharacterTeleport() => _packetsHelper.SendCharacterTeleport(Client, this);

        public void SendUseVehicle(bool success, bool status) => _packetsHelper.SendUseVehicle(Client, success, status);

        public void SendMyShape() => _packetsHelper.SendCharacterShape(Client, this);

        private void TargetChanged(IKillable target)
        {
            if (target is Mob)
            {
                _packetsHelper.SetMobInTarget(Client, (Mob)target);
            }
            else
            {
                _packetsHelper.SetPlayerInTarget(Client, (Character)target);
            }
        }
    }
}
