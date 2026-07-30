using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TinyScript;

#if UNITY_EDITOR
    using UnityEditorInternal;
using System.Linq;
#endif

namespace TinyScript {

    [CreateAssetMenu(fileName = "Loot Table", menuName = "Tiny Slime Studio/Loot Table SO")]
    public class LootDrop : ScriptableObject
    {
        public DropChangeItem[] GuaranteedLootTable = new DropChangeItem[0];
        public DropChangeItem[] OneItemFromList = new DropChangeItem[1];
        public float WeightToNoDrop = 100;

        // Defualt Value
        public int DefualtAdditionalDropCount = 1;
        public float DefualtMinDropRange = 1;
        public float DefualtMaxDropRange = 2;


        // Return List of Guaranteed Drop 
        public List<GameObject> GetGuaranteeedLoot()
        {
            List<GameObject> lootList = new List<GameObject>();

            for (int i = 0; i < GuaranteedLootTable.Length; i++)
            {
                int count = Random.Range(
                    GuaranteedLootTable[i].MinCountItem,
                    GuaranteedLootTable[i].MaxCountItem + 1
                );

                for (int j = 0; j < count; j++)
                {
                    lootList.Add(GuaranteedLootTable[i].Drop);
                }
            }

            return lootList;
        }


        // Return List of Optional Drop 
        public List<GameObject> GetRandomLoot(int chanceCount)
        {
            List<GameObject> lootList = new List<GameObject>();

            // We count the total weight only once
            float totalWeight = WeightToNoDrop;

            for (int i = 0; i < OneItemFromList.Length; i++)
            {
                totalWeight += OneItemFromList[i].Weight;
            }

            for (int j = 0; j < chanceCount; j++)
            {
                float value = Random.Range(0f, totalWeight);

                // No Drop
                if (value < WeightToNoDrop)
                    continue;

                value -= WeightToNoDrop;

                float currentWeight = 0f;

                for (int i = 0; i < OneItemFromList.Length; i++)
                {
                    currentWeight += OneItemFromList[i].Weight;

                    if (value < currentWeight)
                    {
                        int count = Random.Range(
                            OneItemFromList[i].MinCountItem,
                            OneItemFromList[i].MaxCountItem + 1
                        );

                        for (int c = 0; c < count; c++)
                        {
                            lootList.Add(OneItemFromList[i].Drop);
                        }

                        break;
                    }
                }
            }

            return lootList;
        }

        #region Loot Drop Functions

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range |
        /// The radius in the MinRange is excluded from the spawn options
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        /// <param name="AdditionalItemCount">Number of Additional Items</param>
        /// <param name="MinRange">Range Which is Excluded from Spawnpoint</param>
        /// <param name="MaxRange">Max Range of Drop appearing</param>
        /// <param name="SpawnYEqualTo0">Drop appears at the same Y Position as Spawnpoint (Default True)</param>
        public void SpawnDrop(Transform Spawnpoint, int AdditionalItemCount, float MinRange, float MaxRange, bool SpawnYEqualTo0 = true)
        {
            // Generate Loot
            List<GameObject> GuaranteedDropList = GetGuaranteeedLoot();
            List<GameObject> AdditionalDropList = GetRandomLoot(AdditionalItemCount);

            // Spawn Guaranteed Drops
            for (int i = 0; i < GuaranteedDropList.Count; i++)
            {
                // SpawnPosition
                //Vector3 SpawnPosition = new Vector3(Spawnpoint.localPosition.x + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.y + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.z + GenerateRandomNumber(MinRange, MaxRange));
                Vector3 SpawnPosition = GenerateRandomNumber(MinRange, MaxRange);
                SpawnPosition = new Vector3(SpawnPosition.x + Spawnpoint.localPosition.x, SpawnPosition.y + Spawnpoint.localPosition.y, SpawnPosition.z + Spawnpoint.localPosition.z);
                if (SpawnYEqualTo0) { SpawnPosition.y = Spawnpoint.localPosition.y; }

                // Spawn
                Instantiate(GuaranteedDropList[i], SpawnPosition, Quaternion.identity);
            }

            // Spawn Additional Drops
            for (int i = 0; i < AdditionalDropList.Count; i++)
            {
                // SpawnPosition
                //Vector3 SpawnPosition = new Vector3(Spawnpoint.localPosition.x + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.y + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.z + GenerateRandomNumber(MinRange, MaxRange));
                Vector3 SpawnPosition = GenerateRandomNumber(MinRange, MaxRange);
                if (SpawnYEqualTo0) { SpawnPosition.y = Spawnpoint.localPosition.y; }

                // Spawn
                Instantiate(AdditionalDropList[i], SpawnPosition, Quaternion.identity);
            }
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        /// <param name="AdditionalItemCount">Number of Additional Items</param>
        /// <param name="MaxRange">Max Range of Drop appearing</param>
        /// <param name="SpawnYEqualTo0">Drop appears at the same Y Position as Spawnpoint (Default True)</param>
        public void SpawnDrop(Transform Spawnpoint, int AdditionalItemCount, float MaxRange, bool SpawnYEqualTo0 = true)
        {
            SpawnDrop(Spawnpoint, AdditionalItemCount, 0, MaxRange, SpawnYEqualTo0);
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range | Takes Default values for the given Loot Table
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        public void SpawnDropWithDefualtInSphere(Transform Spawnpoint)
        {
            SpawnDrop(Spawnpoint, DefualtAdditionalDropCount, DefualtMinDropRange, DefualtMaxDropRange, false);
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range | Takes Default values for the given Loot Table with Static Position Y
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        public void SpawnDropWithDefualtInCircle(Transform Spawnpoint)
        {
            SpawnDrop(Spawnpoint, DefualtAdditionalDropCount, DefualtMinDropRange, DefualtMaxDropRange, true);
        }

        #endregion

        public void GizmoDrawSpawnRange(Transform Spawnpoint, float MinRange, float MaxRange)
        {
            // The number of points on the circle
            int numPoints = 36;
            float angleIncrement = 360f / numPoints;

            Gizmos.color = Color.green;

            // Draw Max Radius
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleIncrement;
                float x = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * MaxRange;
                float y = Spawnpoint.position.y;
                float z = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * MaxRange;

                Vector3 startPoint = new Vector3(x, y, z);

                // Calculate the next angle
                float nextAngle = (i + 1) * angleIncrement;
                float nextX = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * MaxRange;
                float nextZ = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * MaxRange;

                Vector3 endPoint = new Vector3(nextX, y, nextZ);

                // Draw the gizmo line
                Gizmos.DrawLine(startPoint, endPoint);
            }

            if(MinRange > 0)
            {
                Gizmos.color = Color.red;

                // Draw Max Radius
                for (int i = 0; i < numPoints; i++)
                {
                    float angle = i * angleIncrement;
                    float x = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * MinRange;
                    float y = Spawnpoint.position.y;
                    float z = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * MinRange;

                    Vector3 startPoint = new Vector3(x, y, z);

                    // Calculate the next angle
                    float nextAngle = (i + 1) * angleIncrement;
                    float nextX = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * MinRange;
                    float nextZ = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * MinRange;

                    Vector3 endPoint = new Vector3(nextX, y, nextZ);

                    // Draw the gizmo line
                    Gizmos.DrawLine(startPoint, endPoint);
                }
            }
        }


        // Private Function to Randomize MinRange with MaxRange
        Vector3 GenerateRandomNumber(float innerRadius, float outerRadius)
        {
            Vector3 randomVector;
            Vector3 tmp;

            do
            {
                randomVector = new Vector3(Random.Range(-outerRadius, outerRadius), Random.Range(-outerRadius, outerRadius), Random.Range(-outerRadius, outerRadius));

                tmp = new Vector3(randomVector.x, 0, randomVector.z);

            } while (tmp.magnitude <= innerRadius || tmp.magnitude >= outerRadius);

            return randomVector;
        }
    }

    [System.Serializable]
    public class DropChangeItem
    {
        public Sprite Icon;

        public GameObject Drop;

        public int MinCountItem;
        public int MaxCountItem;

        public float Weight;

    }
}



