using UnityEngine;

//NPCEnums
public enum BodyPart { 
    Head,       Chest,      Hip, 
    ArmRight,   HandRight, 
    ArmLeft,    HandLeft, 
    LegRight,   FootRight,
    LegLeft,    FootLeft,
    Other 
}

public enum NPCLimbHealth {
    DEFAULT = 200,
    Hand = 20,
    LowerArm = 40,
    UpperArm = 50,
    Foot = 20,
    LowerLeg = 60,
    UpperLeg = 70
}

public enum ReactionType { 
    Flinch, 
    Knock, 
    Death, 
    Dismember, 
    explode 
}

public enum LifeState { 
    Alive, 
    Knocked, 
    Dead, 
    Dazed 
}

public enum MovementCondition { 
    Normal  =0, 
    LegLeftLimp =1, 
    LegRightLimp=2,
    Crawl   =3,
    Stunned =4,
    Dazed = 5
}

public enum BehaviourState {
    Patrol,
    Chase,
    Combat,
    Flee,
    Knocked,
    Dead
}

public enum NPCWeaponType {
    Unarmed,
    Meelee,
    Pistol,
    Shotgun,
    Rifle
}

public enum NPCWeaponHand{
    Left,
    Right
}
