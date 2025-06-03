using UnityEngine;

public class Enemy : Troop {
    public override string typeId => data.id;

    public Enemy(TroopData _data, Side _allegiance) : base(_data, _allegiance) {}

    protected override void HandleStateChangeMotion() {
        if (currentState != m_previousState) {
            m_previousState = currentState;
            switch (currentState) {
                case State.Idle:
                    ChangeAnimation("Idle", 0.1f);
                    break;
                case State.Walk:
                    ChangeAnimation("Forward", 0.1f);
                    break;
                case State.KnockedBack:
                    ChangeAnimation("KnockedBack", 0.1f);
                    break;
                case State.Landing:
                    ChangeAnimation("Land", 0.1f);
                    break;
                case State.Die:
                    ChangeSpeed(1);
                    ChangeAnimation("Die", 0.1f);
                    GameplayManager.instance.enemiesRemaining -= 1;
                    break;
            }
        }
    }
};