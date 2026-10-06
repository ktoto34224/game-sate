using System.IO;
using UnityEngine;

namespace PeakClimber
{
    public static class ProceduralSpriteGenerator
    {
        public static Sprite CreateCircleSprite(int radius, Color color, string name = "Circle")
        {
            int size = radius * 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius - 1)
                    {
                        cols[y * size + x] = color;
                    }
                    else if (dist <= radius)
                    {
                        float alpha = 1f - (dist - (radius - 1));
                        cols[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            return sprite;
        }

        public static Sprite CreateRingSprite(int radius, int thickness, Color color, string name = "Ring")
        {
            int size = radius * 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(radius, radius);
            float inner = radius - thickness;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist >= inner && dist <= radius)
                    {
                        cols[y * size + x] = color;
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            return sprite;
        }

        public static Sprite CreateBoxSprite(int width, int height, Color color, Color borderColor, int borderWidth = 2, string name = "Box")
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                    cols[y * width + x] = isBorder ? borderColor : color;
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            return sprite;
        }

        public static Sprite CreateScoutSprite(bool isClimbing = false)
        {
            int w = 64;
            int h = 72;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            void FillRect(int x0, int y0, int bw, int bh, Color c)
            {
                for (int y = y0; y < y0 + bh && y < h; y++)
                {
                    for (int x = x0; x < x0 + bw && x < w; x++)
                    {
                        if (x >= 0 && y >= 0) cols[y * w + x] = c;
                    }
                }
            }

            Color scoutShirt = new Color(0.88f, 0.72f, 0.44f); // Khaki scout uniform
            Color scoutPants = new Color(0.24f, 0.38f, 0.22f); // Dark green cargo shorts
            Color backpack = new Color(0.65f, 0.32f, 0.16f);   // Brown leather backpack
            Color hatColor = new Color(0.28f, 0.45f, 0.25f);   // Green scout campaign hat
            Color skinColor = new Color(0.98f, 0.82f, 0.68f);  // Skin tone
            Color bandana = new Color(0.9f, 0.2f, 0.2f);      // Red neckerchief
            Color boots = new Color(0.18f, 0.14f, 0.12f);      // Hiking boots
            Color eyes = Color.black;

            // Backpack behind player
            FillRect(12, 22, 14, 24, backpack);
            FillRect(14, 24, 10, 20, backpack * 1.15f);

            // Legs / Pants
            if (!isClimbing)
            {
                // Left leg & Right leg standing
                FillRect(24, 6, 6, 16, scoutPants);
                FillRect(34, 6, 6, 16, scoutPants);
                FillRect(22, 0, 9, 7, boots);
                FillRect(33, 0, 9, 7, boots);
            }
            else
            {
                // Climbing legs spread
                FillRect(18, 4, 8, 14, scoutPants);
                FillRect(38, 8, 8, 14, scoutPants);
                FillRect(15, 0, 9, 6, boots);
                FillRect(40, 4, 9, 6, boots);
            }

            // Torso / Shirt
            FillRect(22, 22, 20, 20, scoutShirt);
            // Scout Badges on chest
            FillRect(26, 32, 3, 3, new Color(1f, 0.85f, 0.1f));
            FillRect(30, 32, 3, 3, new Color(0.2f, 0.7f, 1f));
            FillRect(26, 28, 3, 3, new Color(0.9f, 0.3f, 0.2f));

            // Neckerchief / Bandana
            FillRect(27, 40, 10, 5, bandana);
            FillRect(30, 36, 4, 5, bandana);

            // Head / Face
            FillRect(24, 43, 16, 16, skinColor);

            // Eyes (Scout look)
            if (!isClimbing)
            {
                FillRect(33, 49, 3, 4, eyes);
                FillRect(27, 49, 3, 4, eyes);
            }
            else
            {
                // Looking upward with determination
                FillRect(32, 53, 3, 3, eyes);
                FillRect(27, 53, 3, 3, eyes);
            }

            // Scout Campaign Hat
            FillRect(18, 57, 28, 5, hatColor * 0.8f); // Hat brim
            FillRect(23, 62, 18, 9, hatColor);        // Hat crown
            FillRect(23, 60, 18, 3, new Color(0.6f, 0.4f, 0.15f)); // Hat strap

            // Arms & Hands
            if (!isClimbing)
            {
                // Standing arms
                FillRect(18, 24, 5, 14, scoutShirt);
                FillRect(17, 20, 6, 5, skinColor);
                FillRect(41, 24, 5, 14, scoutShirt);
                FillRect(41, 20, 6, 5, skinColor);
            }
            else
            {
                // Climbing arms reaching UP towards rock holds
                FillRect(14, 38, 7, 18, scoutShirt);
                FillRect(12, 54, 8, 8, skinColor); // Left hand grabbing
                FillRect(43, 44, 7, 18, scoutShirt);
                FillRect(44, 60, 8, 8, skinColor); // Right hand reaching higher
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.15f), 50f);
            sprite.name = isClimbing ? "Scout_Climb" : "Scout_Idle";
            return sprite;
        }

        public static Sprite CreateRockSprite(int w, int h, Color baseColor, Color accentColor, Color topGrassColor, bool hasGrass = false)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Procedural craggy stone pattern with subtle noise
                    float n = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                    float n2 = Mathf.PerlinNoise(x * 0.45f + 12.3f, y * 0.45f + 8.7f);
                    Color stone = Color.Lerp(baseColor, accentColor, n * 0.7f + n2 * 0.3f);

                    // Dark borders for crisp chunky platformer look
                    if (x < 2 || x >= w - 2 || y < 2 || y >= h - 2)
                    {
                        stone = stone * 0.7f;
                        stone.a = 1f;
                    }

                    if (hasGrass && y >= h - 6)
                    {
                        // Craggy grass on top
                        float grassNoise = Mathf.Sin(x * 0.8f) * 2f;
                        if (y >= h - 4 + grassNoise)
                        {
                            stone = Color.Lerp(topGrassColor, topGrassColor * 1.2f, n);
                        }
                    }

                    cols[y * w + x] = stone;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 50f);
            sprite.name = "Rock_" + w + "x" + h;
            return sprite;
        }

        public static Sprite CreateMushroomSprite()
        {
            int w = 48;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Vector2 capCenter = new Vector2(24, 24);
            Color capRed = new Color(0.92f, 0.2f, 0.22f);
            Color polkaWhite = Color.white;
            Color stemColor = new Color(0.95f, 0.93f, 0.85f);

            // Stem
            for (int y = 2; y <= 24; y++)
            {
                for (int x = 18; x <= 30; x++)
                {
                    cols[y * w + x] = stemColor;
                }
            }

            // Cap dome
            for (int y = 20; y < 46; y++)
            {
                for (int x = 4; x < 44; x++)
                {
                    float dx = (x - 24) / 18f;
                    float dy = (y - 20) / 22f;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        cols[y * w + x] = capRed;
                    }
                }
            }

            // White dots
            void DrawDot(int cx, int cy, int r)
            {
                for (int y = cy - r; y <= cy + r; y++)
                {
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) <= r)
                        {
                            if (x >= 0 && x < w && y >= 0 && y < h && cols[y * w + x] != Color.clear)
                                cols[y * w + x] = polkaWhite;
                        }
                    }
                }
            }

            DrawDot(24, 38, 4);
            DrawDot(14, 30, 3);
            DrawDot(34, 30, 3);

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.1f), 50f);
            sprite.name = "BouncyMushroom";
            return sprite;
        }

        public static Sprite CreateCampfireSprite(bool isLit = true)
        {
            int w = 48;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color logColor = new Color(0.42f, 0.24f, 0.12f);
            Color stoneColor = new Color(0.45f, 0.45f, 0.45f);

            // Ring of stones
            for (int y = 4; y <= 12; y++)
            {
                for (int x = 8; x <= 40; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y * 2), new Vector2(24, 16)) <= 16)
                    {
                        cols[y * w + x] = stoneColor;
                    }
                }
            }

            // Crossed logs
            for (int i = -10; i <= 10; i++)
            {
                int x1 = 24 + i;
                int y1 = 10 + Mathf.Abs(i) / 2;
                if (x1 >= 0 && x1 < w && y1 >= 0 && y1 < h)
                {
                    cols[y1 * w + x1] = logColor;
                    if (y1 + 1 < h) cols[(y1 + 1) * w + x1] = logColor * 1.2f;
                }
            }

            if (isLit)
            {
                // Vibrant fire flame
                Color fireYellow = new Color(1f, 0.9f, 0.2f);
                Color fireOrange = new Color(1f, 0.45f, 0.1f);
                Color fireRed = new Color(0.9f, 0.15f, 0.1f);

                for (int y = 12; y <= 42; y++)
                {
                    float widthAtY = (42 - y) * 0.4f;
                    for (int x = (int)(24 - widthAtY); x <= (int)(24 + widthAtY); x++)
                    {
                        if (x >= 0 && x < w)
                        {
                            float t = (float)(y - 12) / 30f;
                            Color c = Color.Lerp(fireYellow, fireRed, t);
                            if (Mathf.Abs(x - 24) < widthAtY * 0.5f && t < 0.6f)
                                c = fireYellow;
                            else if (t < 0.7f)
                                c = fireOrange;
                            cols[y * w + x] = c;
                        }
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.1f), 50f);
            sprite.name = isLit ? "Campfire_Lit" : "Campfire_Off";
            return sprite;
        }

        public static Sprite CreatePitonSprite()
        {
            int w = 24;
            int h = 32;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color metal = new Color(0.85f, 0.88f, 0.92f);
            Color darkMetal = new Color(0.4f, 0.45f, 0.5f);
            Color rope = new Color(0.95f, 0.65f, 0.1f);

            // Spike into rock
            for (int y = 14; y <= 24; y++)
            {
                for (int x = 10; x <= 14; x++)
                {
                    cols[y * w + x] = metal;
                }
            }
            // Pointed tip
            cols[25 * w + 12] = darkMetal;
            cols[26 * w + 12] = darkMetal;

            // Carabiner eyelet ring
            for (int y = 4; y <= 16; y++)
            {
                for (int x = 6; x <= 18; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(12, 10));
                    if (d >= 3.5f && d <= 6f)
                    {
                        cols[y * w + x] = rope;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.7f), 50f);
            sprite.name = "Piton";
            return sprite;
        }

        public static Sprite CreateBadgeSprite()
        {
            int w = 36;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color ribbon = new Color(0.2f, 0.5f, 0.9f);
            Color gold = new Color(1f, 0.84f, 0f);
            Color goldShadow = new Color(0.85f, 0.65f, 0f);
            Color starWhite = Color.white;

            // Hanging ribbons
            for (int y = 0; y <= 20; y++)
            {
                for (int x = 8; x <= 14; x++) cols[y * w + x] = ribbon;
                for (int x = 22; x <= 28; x++) cols[y * w + x] = ribbon;
            }

            // Gold Medal circle
            Vector2 center = new Vector2(18, 30);
            for (int y = 16; y <= 44; y++)
            {
                for (int x = 4; x <= 32; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= 14f)
                    {
                        cols[y * w + x] = d > 12f ? goldShadow : gold;
                    }
                }
            }

            // Mountain peak icon on medal
            for (int y = 24; y <= 34; y++)
            {
                int span = (34 - y);
                for (int x = 18 - span; x <= 18 + span; x++)
                {
                    if (x >= 0 && x < w) cols[y * w + x] = starWhite;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 50f);
            sprite.name = "ScoutBadge";
            return sprite;
        }

        public static Sprite CreateEnergyBerrySprite()
        {
            int w = 32;
            int h = 32;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color berryColor = new Color(0.95f, 0.25f, 0.45f);
            Color highlight = new Color(1f, 0.65f, 0.75f);
            Color leaf = new Color(0.2f, 0.75f, 0.3f);

            // Leaf on top
            for (int y = 22; y <= 28; y++)
            {
                for (int x = 12; x <= 22; x++)
                {
                    if (y >= 22 + Mathf.Abs(x - 17)) cols[y * w + x] = leaf;
                }
            }

            // 3 plump berries clustered
            void DrawBerry(int cx, int cy, int r)
            {
                for (int y = cy - r; y <= cy + r; y++)
                {
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (d <= r)
                        {
                            cols[y * w + x] = (x < cx && y > cy) ? highlight : berryColor;
                        }
                    }
                }
            }

            DrawBerry(12, 12, 7);
            DrawBerry(20, 12, 7);
            DrawBerry(16, 18, 6);

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 50f);
            sprite.name = "EnergyBerry";
            return sprite;
        }

        public static Sprite CreateHelicopterSprite()
        {
            int w = 120;
            int h = 60;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color heliBody = new Color(0.95f, 0.8f, 0.15f); // Bright yellow mountain rescue
            Color heliDark = new Color(0.85f, 0.3f, 0.1f);  // Rescue orange stripe
            Color glass = new Color(0.3f, 0.75f, 0.95f, 0.9f);
            Color metal = new Color(0.3f, 0.32f, 0.35f);
            Color white = Color.white;

            // Skids
            for (int x = 40; x <= 95; x++)
            {
                cols[6 * w + x] = metal;
                cols[7 * w + x] = metal;
            }
            // Skid struts
            for (int y = 7; y <= 16; y++)
            {
                cols[y * w + 52] = metal;
                cols[y * w + 82] = metal;
            }

            // Main Cabin
            for (int y = 16; y <= 42; y++)
            {
                for (int x = 45; x <= 100; x++)
                {
                    float dx = (x - 70) / 28f;
                    float dy = (y - 28) / 13f;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        cols[y * w + x] = (y >= 26 && y <= 31) ? heliDark : heliBody;
                    }
                }
            }

            // Cockpit Windshield Glass
            for (int y = 22; y <= 38; y++)
            {
                for (int x = 86; x <= 98; x++)
                {
                    if (cols[y * w + x] != Color.clear)
                    {
                        cols[y * w + x] = glass;
                    }
                }
            }

            // Tail Boom
            for (int y = 26; y <= 32; y++)
            {
                for (int x = 12; x <= 48; x++)
                {
                    cols[y * w + x] = heliBody;
                }
            }

            // Tail Fin & Small Rotor
            for (int y = 24; y <= 44; y++)
            {
                for (int x = 12; x <= 18; x++)
                {
                    if (y >= 24 + (18 - x) * 2) cols[y * w + x] = heliDark;
                }
            }

            // Main Rotor Mast
            for (int y = 42; y <= 50; y++)
            {
                cols[y * w + 69] = metal;
                cols[y * w + 70] = metal;
                cols[y * w + 71] = metal;
            }

            // Rescue Cross
            for (int y = 24; y <= 34; y++)
            {
                for (int x = 58; x <= 66; x++)
                {
                    if (Mathf.Abs(y - 29) <= 1 || Mathf.Abs(x - 62) <= 1)
                        cols[y * w + x] = Color.red;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.4f), 50f);
            sprite.name = "RescueHelicopter";
            return sprite;
        }

        public static Sprite CreateRotorBladeSprite()
        {
            int w = 120;
            int h = 10;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color blade = new Color(0.2f, 0.22f, 0.25f, 0.85f);
            Color tip = Color.yellow;

            for (int y = 2; y <= 7; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    cols[y * w + x] = (x < 12 || x > w - 13) ? tip : blade;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 50f);
            sprite.name = "RotorBlades";
            return sprite;
        }

        public static Sprite CreateHazardSpikesSprite(int count = 4)
        {
            int w = 48;
            int h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color spikeColor = new Color(0.35f, 0.25f, 0.22f);
            Color tipColor = new Color(0.85f, 0.25f, 0.25f);

            int spikeW = w / count;
            for (int s = 0; s < count; s++)
            {
                int startX = s * spikeW;
                int midX = startX + spikeW / 2;
                for (int y = 0; y < h; y++)
                {
                    float widthAtY = (1f - (float)y / h) * (spikeW / 2f);
                    for (int x = (int)(midX - widthAtY); x <= (int)(midX + widthAtY); x++)
                    {
                        if (x >= 0 && x < w)
                        {
                            cols[y * w + x] = (y > h - 6) ? tipColor : spikeColor;
                        }
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 50f);
            sprite.name = "HazardSpikes";
            return sprite;
        }
    }
}
