# ITM-004: Asset HolderNumber validation must consider Hierarchy Role

| Field | Value |
|---|---|
| Status | Done |
| Parent track | [TRK-001](../tracks/TRK-001-sdm-unit-testing.md) |
| Spec | none |
| Collaboration | TBD |
| Assignee | Miguel Garrido |
| Created | 2026-11-06 |

## Problem

A holder slot is identified by (ParentAsset, HierarchyRole, HolderNumber). The validators compared only (ParentAsset, HolderNumber), so a Fan at slot 1 was rejected when a Card already sat at slot 1 of the same parent. `DestinationLocation.HolderNumber` had no `db` check at all (`// todo` in `PlacementValidator`).

## Rules (CAN-001 §3.1.2, §3.1.3)

- `Location.HolderNumber` `db`: slot identity is (ParentAsset, HierarchyRole, HolderNumber); the role is resolved from the asset's Asset Class Device Type; the parent must expose the slot and it must be free.
- `Location.HolderNumber` `batch`: collision when ParentAsset + HolderNumber + AssetClass are all equal.
- `DestinationLocation.HolderNumber` `db`: the parent must have a Holder with that SlotNumber and the asset's Hierarchy Role. No occupancy check.

## Acceptance criteria

1. Same parent, same HolderNumber, different Hierarchy Role is accepted (`db` and `batch`).
2. Same parent, same HolderNumber, same Hierarchy Role is rejected against an existing child (`db`).
3. Two assets in one batch with the same ParentAsset + HolderNumber + AssetClass are rejected (`batch`).
4. Two assets in one batch with the same parent and number but different Asset Classes that resolve to the same role are rejected at `db` level.
5. Destination: accepted when the parent has the Holder (SlotNumber + role), even if that slot is occupied; rejected when the slot is missing, the role differs, the parent is missing, or the Device Type has no role.
6. `dotnet build` and `SDM.AssetManagementTests` are green.

## Out of scope

- Other validators or the Holders definition rules on Asset Class / Asset (§3.1.7).
