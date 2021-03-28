using ProtoBuf;
using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.Server;
using Vintagestory.API.MathTools;

[assembly: ModInfo("quickstep")]

namespace QuickStep {
  public class QuickStepMod : ModSystem {
    private Harmony harmony;
    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);
      capi.Logger.Debug("QuickStep - applying patches");
      harmony = new Harmony("goxmeor.quickstep");
      harmony.PatchAll();
      capi.Logger.Debug("QuickStep - patches applied");
    }
  }

  [HarmonyPatch(typeof(EntityBehaviorControlledPhysics))]
  [HarmonyPatch("tryStep")]
  public class Patch_EntityBehaviorControlledPhysics_tryStep {
    public static bool Prefix(EntityPos pos, Vec3d moveDelta, float dtFac, Cuboidd stepableBox, Cuboidd entityCollisionBox,
      EntityBehaviorControlledPhysics __instance,
      Vec3d ___outposition,
      CollisionTester ___collisionTester,
      Entity ___entity,
      ref bool __result

    ) {
      if (stepableBox == null) return false;

      double heightDiff = stepableBox.Y2 - entityCollisionBox.Y1 + 0.01 * 3f; // This added constant value is a fugly hack because outposition has gravity added, but has not adjusted its position to the ground floor yet
      Vec3d steppos = ___outposition.OffsetCopy(moveDelta.X, heightDiff, moveDelta.Z);
      bool canStep = !___collisionTester.IsColliding(___entity.World.BlockAccessor, ___entity.CollisionBox, steppos, false);

      if (canStep) {
        // pos.Y += 0.07 * dtFac; // REMOVED
        pos.Y += heightDiff; // ADDED
        ___collisionTester.ApplyTerrainCollision(___entity, pos, dtFac, ref ___outposition);
        __result = true;

      }
      else {
        __result = false;
      }

      // HARMONY SKIP
      return false;
    }
  }
}
