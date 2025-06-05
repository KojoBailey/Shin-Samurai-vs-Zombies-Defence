using UnityEngine;

public class Enemy : Troop {
    public override string typeId => data.id;

    public Enemy(TroopData _data, Side _allegiance) : base(_data, _allegiance) {}

    protected override void HandleDeath() {
        base.HandleDeath();
        GameplayManager.instance.enemiesRemaining -= 1;
    }
};