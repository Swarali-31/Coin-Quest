using UnityEngine;

namespace CoinQuest
{
    /// <summary>Creates the game's stylized articulated character visuals without external assets.</summary>
    public static class CharacterModelFactory
    {
        private static Material Mat(Color color, bool emissive = false)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.22f);
            if (emissive) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 1.7f); }
            return material;
        }

        public static Transform CreateExplorer(Transform parent, bool female)
        {
            var visual = Root(parent, female ? "Female Explorer Visual" : "Male Explorer Visual");
            // The Player root is aligned to the CharacterController's floor; keep the visual feet on that floor.
            visual.localPosition = Vector3.zero;
            var skin = Mat(new Color(0.77f, 0.48f, 0.3f));
            var shirt = Mat(female ? new Color(0.06f, 0.55f, 0.52f) : new Color(0.08f, 0.29f, 0.72f));
            var pants = Mat(new Color(0.24f, 0.17f, 0.13f));
            var leather = Mat(new Color(0.31f, 0.17f, 0.09f));
            var leatherLight = Mat(new Color(0.48f, 0.29f, 0.14f));
            var hair = Mat(female ? new Color(0.26f, 0.12f, 0.045f) : new Color(0.035f, 0.025f, 0.02f));
            var eye = Mat(new Color(0.12f, 0.075f, 0.045f));
            var stone = Mat(new Color(0.08f, 0.72f, 1f), true);

            // Sphere scale is full width/height/depth in Unity. These pieces intentionally overlap at
            // their seams to read as one compact, coherent two-metre explorer silhouette.
            Capsule(visual, "Shirt Torso", new Vector3(0, 1.12f, 0), new Vector3(0.46f, 0.58f, 0.3f), shirt);
            Ellipsoid(visual, "Trouser Waist", new Vector3(0, 0.76f, 0), new Vector3(0.39f, 0.28f, 0.27f), pants);
            Box(visual, "Explorer Belt", new Vector3(0, 0.88f, 0.015f), new Vector3(0.4f, 0.065f, 0.29f), leather);
            Box(visual, "Brass Belt Buckle", new Vector3(0, 0.88f, 0.164f), new Vector3(0.075f, 0.055f, 0.025f), Mat(new Color(0.92f, 0.62f, 0.16f)));
            Capsule(visual, "Neck", new Vector3(0, 1.42f, 0), new Vector3(0.15f, 0.18f, 0.14f), skin);

            var head = Ellipsoid(visual, "Explorer Head", new Vector3(0, 1.63f, 0.015f), new Vector3(0.39f, 0.42f, 0.36f), skin).transform;
            Ellipsoid(visual, "Left Ear", new Vector3(-0.195f, 1.63f, 0.005f), new Vector3(0.09f, 0.13f, 0.09f), skin);
            Ellipsoid(visual, "Right Ear", new Vector3(0.195f, 1.63f, 0.005f), new Vector3(0.09f, 0.13f, 0.09f), skin);
            // Hair cap, back locks and fringe frame the face instead of covering it.
            Ellipsoid(visual, "Hair Cap", new Vector3(0, 1.84f, -0.005f), new Vector3(0.4f, 0.17f, 0.37f), hair);
            Ellipsoid(visual, "Hair Back", new Vector3(0, 1.64f, -0.13f), new Vector3(0.36f, 0.31f, 0.13f), hair);
            if (female)
            {
                Ellipsoid(visual, "Ponytail Tie", new Vector3(0, 1.59f, -0.25f), Vector3.one * 0.09f, leatherLight);
                Ellipsoid(visual, "Ponytail Upper", new Vector3(0, 1.43f, -0.32f), new Vector3(0.15f, 0.25f, 0.16f), hair).transform.localRotation = Quaternion.Euler(-25, 0, 0);
                Ellipsoid(visual, "Ponytail End", new Vector3(0, 1.24f, -0.37f), new Vector3(0.12f, 0.2f, 0.13f), hair).transform.localRotation = Quaternion.Euler(-25, 0, 0);
                Ellipsoid(visual, "Side Fringe", new Vector3(0.1f, 1.76f, 0.18f), new Vector3(0.18f, 0.09f, 0.09f), hair).transform.localRotation = Quaternion.Euler(0, 0, 15);
            }
            else
            {
                for (int i = -1; i <= 1; i++)
                    Capsule(visual, "Spiky Hair", new Vector3(i * 0.14f, 1.92f, -0.01f), new Vector3(0.12f, 0.18f, 0.13f), hair).transform.localRotation = Quaternion.Euler(0, 0, i * -20f);
                Ellipsoid(visual, "Side Fringe", new Vector3(-0.1f, 1.76f, 0.18f), new Vector3(0.17f, 0.09f, 0.09f), hair).transform.localRotation = Quaternion.Euler(0, 0, -15);
            }
            foreach (float x in new[] { -0.052f, 0.052f })
            {
                Ellipsoid(visual, "Eye White", new Vector3(x * 1.45f, 1.65f, 0.19f), new Vector3(0.09f, 0.075f, 0.035f), Mat(Color.white));
                Ellipsoid(visual, "Eye", new Vector3(x * 1.45f, 1.65f, 0.218f), new Vector3(0.042f, 0.052f, 0.022f), eye);
                Box(visual, "Eyebrow", new Vector3(x * 1.45f, 1.73f, 0.19f), new Vector3(0.1f, 0.025f, 0.02f), hair);
            }
            Ellipsoid(visual, "Nose", new Vector3(0, 1.59f, 0.224f), new Vector3(0.07f, 0.085f, 0.07f), skin);
            Box(visual, "Mouth", new Vector3(0, 1.51f, 0.195f), new Vector3(0.1f, 0.018f, 0.015f), Mat(new Color(0.35f, 0.12f, 0.1f)));

            // A real articulated hierarchy: shoulder, elbow, wrist, hip and knee pivots.
            Transform leftArm = BuildExplorerArm(visual, -1, skin, shirt, leather);
            Transform rightArm = BuildExplorerArm(visual, 1, skin, shirt, leather);
            Transform leftLeg = BuildExplorerLeg(visual, -1, pants, leather, leatherLight);
            Transform rightLeg = BuildExplorerLeg(visual, 1, pants, leather, leatherLight);

            // Backpack sits behind the torso; curved straps and flap remain attached to this visual root.
            Box(visual, "Explorer Backpack", new Vector3(0, 1.17f, -0.22f), new Vector3(0.34f, 0.43f, 0.2f), leather);
            Box(visual, "Backpack Flap", new Vector3(0, 1.35f, -0.329f), new Vector3(0.29f, 0.12f, 0.035f), leatherLight);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(visual, "Backpack Shoulder Strap", new Vector3(side * 0.16f, 1.27f, -0.075f), new Vector3(0.055f, 0.38f, 0.045f), leatherLight);
            }
            var crystal = Ellipsoid(visual, "Glowing Echo Stone", new Vector3(0.24f, 1.35f, -0.205f), new Vector3(0.09f, 0.15f, 0.09f), stone);
            crystal.transform.localRotation = Quaternion.Euler(0, 0, 12);

            var animator = visual.gameObject.AddComponent<ExplorerVisualAnimator>();
            animator.Configure(parent, leftArm, rightArm, leftLeg, rightLeg, head);
            return visual;
        }

        private static Transform BuildExplorerArm(Transform root, int side, Material skin, Material sleeve, Material leather)
        {
            float x = side * 0.25f;
            var shoulder = Pivot(root, side < 0 ? "Left Shoulder" : "Right Shoulder", new Vector3(x, 1.31f, 0));
            Capsule(shoulder, "Short Sleeve", new Vector3(side * 0.035f, -0.11f, 0), new Vector3(0.18f, 0.24f, 0.18f), sleeve);
            Capsule(shoulder, "Upper Arm", new Vector3(side * 0.04f, -0.3f, 0), new Vector3(0.14f, 0.3f, 0.14f), skin);
            var elbow = Pivot(shoulder, "Elbow", new Vector3(side * 0.05f, -0.37f, 0));
            Capsule(elbow, "Forearm", new Vector3(side * 0.01f, -0.17f, 0.01f), new Vector3(0.13f, 0.27f, 0.13f), skin);
            Ellipsoid(elbow, "Wrist Cuff", new Vector3(side * 0.012f, -0.31f, 0), new Vector3(0.15f, 0.09f, 0.15f), leather);
            Ellipsoid(elbow, "Hand", new Vector3(side * 0.014f, -0.39f, 0.02f), new Vector3(0.14f, 0.15f, 0.13f), skin);
            return shoulder;
        }

        private static Transform BuildExplorerLeg(Transform root, int side, Material pants, Material boot, Material sole)
        {
            float x = side * 0.12f;
            var hip = Pivot(root, side < 0 ? "Left Hip" : "Right Hip", new Vector3(x, 0.72f, 0));
            Capsule(hip, "Trouser Thigh", new Vector3(0, -0.16f, 0), new Vector3(0.2f, 0.35f, 0.2f), pants);
            var knee = Pivot(hip, "Knee", new Vector3(0, -0.32f, 0));
            Capsule(knee, "Lower Leg", new Vector3(0, -0.16f, 0), new Vector3(0.17f, 0.32f, 0.18f), pants);
            Ellipsoid(knee, "Boot Cuff", new Vector3(0, -0.31f, 0.025f), new Vector3(0.2f, 0.12f, 0.2f), boot);
            Box(knee, "Adventure Boot", new Vector3(0, -0.31f, 0.09f), new Vector3(0.2f, 0.14f, 0.29f), boot);
            Box(knee, "Boot Sole", new Vector3(0, -0.37f, 0.09f), new Vector3(0.21f, 0.05f, 0.29f), sole);
            return hip;
        }

        public static Transform CreateGuardian(Transform parent, out Transform leftArm, out Transform rightArm, out Transform leftLeg, out Transform rightLeg)
        {
            var visual = Root(parent, "Stone Guardian Visual");
            var stone = Mat(new Color(0.29f, 0.31f, 0.29f));
            var stoneLight = Mat(new Color(0.43f, 0.42f, 0.36f));
            var moss = Mat(new Color(0.18f, 0.3f, 0.19f));
            var crack = Mat(new Color(1f, 0.32f, 0.045f), true);
            var dark = Mat(new Color(0.12f, 0.13f, 0.12f));

            Ellipsoid(visual, "Broad Stone Chest", new Vector3(0, 1.45f, 0), new Vector3(1.15f, 0.95f, 0.68f), stone);
            Ellipsoid(visual, "Stone Pelvis", new Vector3(0, 0.73f, 0), new Vector3(0.55f, 0.32f, 0.38f), stone);
            Ellipsoid(visual, "Stone Neck", new Vector3(0, 2.25f, 0), new Vector3(0.25f, 0.25f, 0.25f), stoneLight);
            Ellipsoid(visual, "Stone Head", new Vector3(0, 2.56f, 0.015f), new Vector3(0.39f, 0.38f, 0.34f), stoneLight);
            Ellipsoid(visual, "Heavy Brow", new Vector3(0, 2.68f, 0.29f), new Vector3(0.4f, 0.13f, 0.12f), stone);
            for (int side = -1; side <= 1; side += 2)
            {
                Ellipsoid(visual, "Guardian Eye", new Vector3(side * 0.15f, 2.59f, 0.32f), new Vector3(0.075f, 0.055f, 0.045f), crack);
                Ellipsoid(visual, "Shoulder Armor", new Vector3(side * 0.66f, 2.02f, 0), new Vector3(0.36f, 0.34f, 0.39f), stoneLight);
                Ellipsoid(visual, "Chest Armor Plate", new Vector3(side * 0.33f, 1.66f, 0.34f), new Vector3(0.32f, 0.4f, 0.13f), stoneLight);
                Ellipsoid(visual, "Moss Patch", new Vector3(side * 0.56f, 2.1f, -0.08f), new Vector3(0.16f, 0.12f, 0.08f), moss);
            }
            Box(visual, "Carved Stone Mouth", new Vector3(0, 2.4f, 0.34f), new Vector3(0.25f, 0.045f, 0.035f), dark);
            for (int i = 0; i < 3; i++)
                Box(visual, "Glowing Stone Crack", new Vector3((i - 1) * 0.1f, 1.54f - i * 0.1f, 0.438f), new Vector3(0.025f, 0.19f, 0.014f), crack).transform.localRotation = Quaternion.Euler(0, 0, 18f * (i - 1));

            leftArm = BuildGuardianArm(visual, -1, stone, stoneLight);
            rightArm = BuildGuardianArm(visual, 1, stone, stoneLight);
            leftLeg = BuildGuardianLeg(visual, -1, stone, stoneLight);
            rightLeg = BuildGuardianLeg(visual, 1, stone, stoneLight);
            var animator = parent.gameObject.AddComponent<GuardianVisualAnimator>();
            animator.Configure(parent, leftArm, rightArm, leftLeg, rightLeg, visual);
            return visual;
        }

        private static Transform BuildGuardianArm(Transform visual, int side, Material stone, Material light)
        {
            var shoulder = Pivot(visual, "Guardian Shoulder", new Vector3(side * 0.73f, 2f, 0));
            Ellipsoid(shoulder, "Massive Upper Arm", new Vector3(side * 0.12f, -0.27f, 0), new Vector3(0.24f, 0.31f, 0.25f), stone);
            var elbow = Pivot(shoulder, "Guardian Elbow", new Vector3(side * 0.16f, -0.5f, 0));
            Ellipsoid(elbow, "Stone Forearm", new Vector3(side * 0.04f, -0.25f, 0), new Vector3(0.27f, 0.32f, 0.28f), light);
            Ellipsoid(elbow, "Stone Fist", new Vector3(side * 0.05f, -0.57f, 0.04f), new Vector3(0.28f, 0.23f, 0.27f), stone);
            return shoulder;
        }

        private static Transform BuildGuardianLeg(Transform visual, int side, Material stone, Material light)
        {
            var hip = Pivot(visual, "Guardian Hip", new Vector3(side * 0.3f, 1.05f, 0));
            Ellipsoid(hip, "Stone Thigh", new Vector3(0, -0.2f, 0), new Vector3(0.28f, 0.34f, 0.3f), stone);
            var knee = Pivot(hip, "Guardian Knee", new Vector3(0, -0.42f, 0));
            Ellipsoid(knee, "Stone Shin", new Vector3(0, -0.2f, 0), new Vector3(0.23f, 0.42f, 0.25f), light);
            Ellipsoid(knee, "Stone Foot", new Vector3(0, -0.43f, 0.1f), new Vector3(0.29f, 0.2f, 0.4f), stone);
            return hip;
        }

        public static Transform CreateShadowBat(Transform parent, out Transform leftWing, out Transform rightWing)
        {
            var visual = Root(parent, "Shadow Bat Visual");
            var purple = Mat(new Color(0.19f, 0.08f, 0.28f));
            var membrane = Mat(new Color(0.31f, 0.12f, 0.43f));
            var dark = Mat(new Color(0.08f, 0.055f, 0.12f));
            var glow = Mat(new Color(0.82f, 0.16f, 1f), true);
            Ellipsoid(visual, "Bat Body", new Vector3(0, 0.2f, 0), new Vector3(0.27f, 0.34f, 0.22f), purple);
            Ellipsoid(visual, "Bat Head", new Vector3(0, 0.52f, 0.06f), new Vector3(0.28f, 0.25f, 0.24f), dark);
            for (int side = -1; side <= 1; side += 2)
            {
                Ellipsoid(visual, "Pointed Bat Ear", new Vector3(side * 0.16f, 0.67f, 0), new Vector3(0.11f, 0.3f, 0.075f), purple).transform.localRotation = Quaternion.Euler(0, 0, side * -17f);
                Ellipsoid(visual, "Bat Eye", new Vector3(side * 0.115f, 0.54f, 0.265f), new Vector3(0.055f, 0.05f, 0.035f), glow);
                Ellipsoid(visual, "Bat Fang", new Vector3(side * 0.07f, 0.37f, 0.27f), new Vector3(0.025f, 0.07f, 0.025f), Mat(Color.white));
                Ellipsoid(visual, "Bat Claw", new Vector3(side * 0.1f, -0.12f, 0.08f), new Vector3(0.035f, 0.09f, 0.04f), dark);
            }
            leftWing = BuildBatWing(visual, -1, purple, membrane);
            rightWing = BuildBatWing(visual, 1, purple, membrane);
            var animator = parent.gameObject.AddComponent<BatVisualAnimator>(); animator.Configure(leftWing, rightWing);
            return visual;
        }

        private static Transform BuildBatWing(Transform visual, int side, Material edge, Material membrane)
        {
            var pivot = Pivot(visual, side < 0 ? "Left Wing Pivot" : "Right Wing Pivot", new Vector3(side * 0.2f, 0.35f, 0));
            var wing = Ellipsoid(pivot, "Bat Wing Membrane", new Vector3(side * 0.4f, -0.06f, 0), new Vector3(0.82f, 0.48f, 0.035f), membrane);
            wing.transform.localRotation = Quaternion.Euler(0, 0, side * -18f);
            Ellipsoid(pivot, "Wing Leading Edge", new Vector3(side * 0.42f, 0.03f, 0.025f), new Vector3(0.8f, 0.055f, 0.045f), edge).transform.localRotation = Quaternion.Euler(0, 0, side * -18f);
            for (int i = 1; i <= 2; i++)
                Box(pivot, "Bat Wing Finger", new Vector3(side * (0.2f + i * 0.19f), -0.06f - i * 0.045f, 0.035f), new Vector3(0.035f, 0.36f - i * 0.07f, 0.03f), edge).transform.localRotation = Quaternion.Euler(0, 0, side * (45f + i * 12f));
            return pivot;
        }

        private static Transform Root(Transform parent, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
        }
        private static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position; return go.transform;
        }
        private static GameObject Ellipsoid(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; Object.Destroy(go.GetComponent<Collider>()); return go;
        }
        private static GameObject Capsule(Transform parent, string name, Vector3 position, Vector3 dimensions, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = new Vector3(dimensions.x, dimensions.y * 0.5f, dimensions.z); go.GetComponent<Renderer>().sharedMaterial = material; Object.Destroy(go.GetComponent<Collider>()); return go;
        }
        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; Object.Destroy(go.GetComponent<Collider>()); return go;
        }
    }
}
