// MIT License
//
// Copyright (c) 2021 SirRandoo
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace SirRandoo.DebugListerOptimizer
{
    [HarmonyPatch]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class DebugListerPatch
    {
        private static readonly ConstructorInfo OldClassConstructor;
        private static readonly ConstructorInfo NewClassConstructor;
        private static readonly Type OldClassType;
        private static readonly Type NewClassType;

        static DebugListerPatch()
        {
            NewClassType = typeof(Dialog_DebugOptionListLister);
            OldClassType = typeof(Dialog_DebugOptionListLister);
            NewClassConstructor = AccessTools.Constructor(
                typeof(DebugListListerDialog),
                new[] {typeof(IEnumerable<DebugMenuOption>)}
            );
            OldClassConstructor = AccessTools.Constructor(
                typeof(Dialog_DebugOptionListLister),
                new[] {typeof(IEnumerable<DebugMenuOption>)}
            );
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SpawnPawn");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SpawnWeapon");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SpawnApparel");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceNearThing");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceNearStacksOf25");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceNearStacksOf75");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceDirectThing");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceDirectStackOf25");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryAddToInventory");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SpawnThingWithWipeMode");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SetTerrain");

        #if RW13
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceNearThingWithStyle");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "TryPlaceNearMarketValue");
            yield return AccessTools.Method(typeof(DebugToolsSpawning), "SetTerrainRect");
        #endif
        }

        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj && instruction.OperandIs(OldClassConstructor))
                {
                    instruction.operand = NewClassConstructor;
                }
                else if (instruction.opcode == OpCodes.Ldtoken && instruction.OperandIs(OldClassType))
                {
                    instruction.operand = NewClassType;
                }

                yield return instruction;
            }
        }
    }
}
