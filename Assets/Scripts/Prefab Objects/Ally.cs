public class Ally : Troop {
    public override string typeId => data.id;
    protected override bool isAlly => true;

    public Ally(TroopData _data, Side _allegiance) : base(_data, _allegiance) {}
}