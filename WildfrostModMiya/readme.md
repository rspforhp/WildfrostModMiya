This is the api for adding cards, and in future more to the game wildfrost:
This api can be used in multiple ways: Csharp and JSON

Here are some examples of how to use it in csharp:
WildfrostModMiya.CardAdder.OnAskForAddingCards += delegate (int i)
{
CardAdder.CreateCardData("modguid","internalname")\
.SetTitle("Name you see in game")\
//.SetIsItem() or .SetIsUnit() \
.AddToPool(CardAdder.VanillaRewardPools.BasicItemPool) \
.SetCanPlay(CardAdder.CanPlay.CanPlayOnEnemy | CardAdder.CanPlay.CanPlayOnBoard)\
.SetSprites("CardPortraits\\testPortrait","CardPortraits\\testBackground")\
.SetDamage(2)\
.SetBloodProfile(CardAdder.VanillaBloodProfiles.BloodProfilePinkWisp)\
.SetIdleAnimationProfile(CardAdder.VanillaCardAnimationProfiles.GoopAnimationProfile)\
.SetTraits(CardAdder.VanillaTraits.Barrage.TraitStack(1))\
.SetStartWithEffects(CardAdder.VanillaStatusEffects.IncreaseEffects.StatusEffectStack(1))\
.SetAttackEffects(CardAdder.VanillaStatusEffects.Demonize.StatusEffectStack(1))\
.RegisterCardInApi();\
};


Here are example of how to use it in json:\
{\
"portraitPath": "CardPortraits\\testPortrait",\
"backgroundPath": "CardPortraits\\testBackground",\
"name": "API.TestCard",\
"title": "Test Card",\
"hp": 0,\
"hasHealth": false,\
"damage": 1,\
"hasAttack": true,\
"counter": 0,\
"upgrades": [\
	],\
	"attackEffects": [\
		{\
			"name": "Demonize",\
			"count": 1\
		}\
	],\
	"startWithEffects": [\
	],\
	"traits": [\
	],\
	"customData": {\
	},\
	"pools": [\
		"BasicItemPool"\
	],\
	"bloodProfile": "Blood Profile Normal",\
	"idleAnimation": "SwayAnimationProfile",\
	"CardType": "Item",\
	"IsItem": true,\
	"CanPlayOnBoard": true,\
	"CanPlayOnEnemy": true,\
	"CanPlayOnFriendly": true,\
	"CanPlayOnHand": true\
}



Next ill show some variable stuff you might want to know for json modding:\
VanillaBloodProfiles\
{\
Blood Profile Berry,\
Blood Profile Black,\
Blood Profile BlueDouble,\
Blood Profile Fungus,\
Blood Profile Normal,\
Blood Profile Snow,\
Blood Profile PinkWisp,\
Blood Profile Husk,\
}

VanillaCardAnimationProfiles\
{\
HeartbeatAnimationProfile,\
FloatAnimationProfile,\
FloatSquishAnimationProfile,\
FlyAnimationProfile,\
GiantAnimationProfile,\
HangAnimationProfile,\
Heartbeat2AnimationProfile,\
PingAnimationProfile,\
PulseAnimationProfile,\
ShakeAnimationProfile,\
SquishAnimationProfile,\
SwayAnimationProfile,\
GoopAnimationProfile,\
}

VanillaTraits\
{\
Aimless,\
Backline,\
Barrage,\
Bombard 1,\
Bombard 2,\
Combo,\
Consume,\
Crush,\
Draw,\
Effigy,\
Explode,\
Frontline,\
Fury,\
Greed,\
Hellbent,\
Knockback,\
Longshot,\
Noomlin,\
Pigheaded,\
Pull,\
Recycle,\
Smackback,\
Soulbound,\
Spark,\
Summoned,\
Trash,\
Unmovable,\
Wild,\
}

The list of effects is giant, but i'll show you the most important ones:

VanillaStatusEffects{\
Block,\
Demonize,\
Frost,\
Haze,\
ImmuneToFrost,\
ImmuneToSnow,\
ImmuneToSpice,\
ImmuneToVim,\
On Hit Pull Target,\
On Hit Push Target,\
Pull,\
Push,\
ResistShroom,\
ResistSnow,\
ResistSpice,\
Scrap,\
Shroom,\
Snow,\
Spice,\
Teeth,\
Summoned,\
Unmovable,\
Weakness,\
FOR MORE ASK IN DISCORD\
}

VanillaCardUpgrades\
{\
None,\
CardUpgradeAcorn,\
CardUpgradeAttackAndHealth,\
CardUpgradeAttackConsume,\
CardUpgradeAttackIncreaseCounter,\
CardUpgradeAttackRemoveEffects,\
CardUpgradeBalanced,\
CardUpgradeBarrage,\
CardUpgradeBattle,\
CardUpgradeBling,\
CardUpgradeBlock,\
CardUpgradeBom,\
CardUpgradeBombskull,\
CardUpgradeBoost,\
CardUpgradeCake,\
CardUpgradeCloudberry,\
CardUpgradeConsume,\
CardUpgradeConsumeAddHealth,\
CardUpgradeConsumeOverload,\
CardUpgradeCritical,\
CardUpgradeCrush,\
CardUpgradeDemonize,\
CardUpgradeDraw,\
CardUpgradeEffigy,\
CardUpgradeFrenzyConsume,\
CardUpgradeFrenzyReduceAttack,\
CardUpgradeFrosthand,\
CardUpgradeFury,\
CardUpgradeGreed,\
CardUpgradeHeart,\
CardUpgradeHook,\
CardUpgradeInk,\
CardUpgradeNoomlin,\
CardUpgradeOverload,\
CardUpgradePig,\
CardUpgradePunchfist,\
CardUpgradeRemoveCharmLimit,\
CardUpgradeScrap,\
CardUpgradeShellBecomesSpice,\
CardUpgradeShellOnKill,\
CardUpgradeShroom,\
CardUpgradeShroomReduceHealth,\
CardUpgradeSnowball,\
CardUpgradeSnowImmune,\
CardUpgradeSpark,\
CardUpgradeSpice,\
CardUpgradeSpiky,\
CardUpgradeSun,\
CardUpgradeTeethWhenHit,\
CardUpgradeTrash,\
CardUpgradeWeakness,\
CardUpgradeWildcard,\
Crown,\
}