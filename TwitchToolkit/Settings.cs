﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace TwitchToolkit
{
    public class Settings : ModSettings
    {
        public static string Channel = "";
        public static string Username = "";
        public static string OAuth = "";

        public static int VoteInterval = 5;
        public static int VoteTime = 1;
        public static int VoteOptions = 3;
        public static bool VoteEnabled = true;
        public static bool AutoConnect = true;
        public static bool OtherStorytellersEnabled = true;
        public static bool CommandsModsEnabled = true;
        public static bool CommandsAliveEnabled = true;
        public static bool QuotesEnabled = true;

        public static int CoinInterval = 3;
        public static int CoinAmount = 15;
        public static int MinimumPurchasePrice = 50;
        public static int KarmaCap = 300;

        public static bool EarningCoins = true;
        public static bool StoreOpen = true;

        // viewer storage
        public static Dictionary<string, int> ViewerIds = new Dictionary<string, int>();
        public static Dictionary<int, int> ViewerCoins = new Dictionary<int, int>();
        public static Dictionary<int, int> ViewerKarma = new Dictionary<int, int>();

        public static List<Viewer> listOfViewers;

        // product storage
        public static Dictionary<string, int> ProductIds = new Dictionary<string, int>();
        public static Dictionary<int, int> ProductTypes = new Dictionary<int, int>();
        public static Dictionary<int, string> ProductNames = new Dictionary<int, string>();
        public static Dictionary<int, int> ProductKarmaTypes = new Dictionary<int, int>();
        public static Dictionary<int, int> ProductAmounts = new Dictionary<int, int>();
        public static Dictionary<int, int> ProductEventIds = new Dictionary<int, int>();

        public static List<Product> products = null;

        // item storage
        public static Dictionary<string, int> ItemIds = new Dictionary<string, int>();
        public static Dictionary<int, int> ItemPrices = new Dictionary<int, int>();
        public static Dictionary<int, string> ItemDefnames = new Dictionary<int, string>();
        public static Dictionary<int, string> ItemStuffnames = new Dictionary<int, string>();

        public static List<Item> items = null;

        private static List<string> _Categories = Enum.GetNames(typeof(EventCategory)).ToList();
        public static List<int> CategoryWeights = Enumerable.Repeat<int>(100, _Categories.Count).ToList();

        public static double CategoryWeight(EventCategory category)
        {
            var index = _Categories.IndexOf(Enum.GetName(typeof(EventCategory), category));
            if (index < 0 || index >= CategoryWeights.Count)
            {
                return 1;
            }

            return CategoryWeights[index] / 100.0;
        }

        private static Dictionary<int, string> _Events = Events.GetEvents().ToDictionary(e => e.Id, e => e.Description);
        public static Dictionary<int, int> EventWeights = Events.GetEvents().ToDictionary(e => e.Id, e => 100);

        public static double EventWeight(int id)
        {
            if (!EventWeights.ContainsKey(id))
            {
                return 1;
            }

            return EventWeights[id] / 100.0;
        }

        public void Save()
        {
            LoadedModManager.WriteModSettings(this.Mod.Content.Identifier, this.Mod.GetType().Name, this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Channel, "Channel", "", true);
            Scribe_Values.Look(ref Username, "Username", "", true);
            Scribe_Values.Look(ref OAuth, "OAuth", "", true);

            Scribe_Values.Look(ref VoteTime, "VoteTime", 1, true);
            Scribe_Values.Look(ref VoteOptions, "VoteOptions", 3, true);
            Scribe_Values.Look(ref VoteEnabled, "VoteEnabled", false, true);
            Scribe_Values.Look(ref AutoConnect, "AutoConnect", false, true);
            Scribe_Values.Look(ref OtherStorytellersEnabled, "OtherStorytellersEnabled", false, true);
            Scribe_Values.Look(ref CommandsModsEnabled, "CommandsModsEnabled", true, true);
            Scribe_Values.Look(ref CommandsAliveEnabled, "CommandsAliveEnabled", true, true);
            Scribe_Values.Look(ref QuotesEnabled, "QuotesEnabled", true, true);

            Scribe_Values.Look(ref VoteInterval, "VoteInterval", 5, true);
            Scribe_Values.Look(ref CoinInterval, "CoinInterval", 3, true);
            Scribe_Values.Look(ref CoinAmount, "CoinAmount", 15, true);
            Scribe_Values.Look(ref KarmaCap, "KarmaCap", 300, true);

            Scribe_Values.Look(ref EarningCoins, "EarningCoins", false, true);
            Scribe_Values.Look(ref StoreOpen, "StoreOpen", false, true);

            Scribe_Collections.Look(ref ViewerIds, "ViewerIds", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ViewerCoins, "ViewerCoins", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ViewerKarma, "ViewerKarma", LookMode.Value, LookMode.Value);

            Scribe_Collections.Look(ref ItemIds, "ItemIds", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ItemPrices, "ItemPrices", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ItemDefnames, "ItemDefnames", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ItemStuffnames, "ItemStuffnames", LookMode.Value, LookMode.Value);

            Scribe_Collections.Look(ref ProductIds, "ProductIds", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ProductTypes, "ProductTypes", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ProductNames, "ProductNames", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ProductKarmaTypes, "ProductKarmaTypes", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ProductAmounts, "ProductAmounts", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ProductEventIds, "ProductEventIds", LookMode.Value, LookMode.Value);

            Scribe_Collections.Look(ref CategoryWeights, "CategoryWeights", LookMode.Value);


            if (ViewerIds == null)
            {
                ViewerIds = new Dictionary<string, int>();
                ViewerCoins = new Dictionary<int, int>();
                ViewerKarma = new Dictionary<int, int>();
            }


            if (CategoryWeights == null)
            {
                CategoryWeights = Enumerable.Repeat<int>(100, _Categories.Count).ToList();
            }

            Scribe_Collections.Look(ref EventWeights, "EventWeights", LookMode.Value);
            if (EventWeights == null)
            {
                EventWeights = Events.GetEvents().ToDictionary(e => e.Id, e => 100);
            }

            if (listOfViewers == null)
            {
                listOfViewers = new List<Viewer>();
                foreach (KeyValuePair<string, int> viewer in ViewerIds)
                {
                    int viewerkarma = ViewerKarma[viewer.Value];
                    int viewercoins = ViewerCoins[viewer.Value];
                    Viewer newviewer = new Viewer(viewer.Key, viewer.Value);
                    listOfViewers.Add(newviewer);
                }
            }

            if (products == null)
            {
                if (ProductIds == null)
                {
                    Helper.Log("Ressetting Products");
                    ResetProductData();
                }
                else
                {
                    Helper.Log("Loading Product Settings");
                    products = new List<Product>();
                    // load products from settings, then load them into settings class
                    foreach (KeyValuePair<string, int> product in ProductIds)
                    {
                        int id = product.Value;
                        string abr = product.Key;
                        int type = ProductTypes[id];
                        string name = ProductNames[id];
                        int karmatype = ProductKarmaTypes[id];
                        int amount = ProductAmounts[id];
                        int evtId = ProductEventIds[id];
                        products.Add(new Product(id, type, name, abr, karmatype, amount, evtId));
                    }
                }
            }

            if (items == null)
            {
                if (ItemIds == null)
                {
                    ResetItemData();
                }
                else
                {
                    items = new List<Item>();
                    
                    foreach(KeyValuePair<string, int> item in ItemIds)
                    {
                        int id = item.Value;
                        string abr = item.Key;
                        int price = ItemPrices[id];
                        string defname = ItemDefnames[id];
                        string stuffname = ItemStuffnames[id];
                        items.Add(new Item(price, abr, defname, id, stuffname));
                    }
                }
            }
        }

        private static int _showOAuth = 0;
        public static void HideOAuth()
        {
            _showOAuth = 0;
        }

        private static float _padding = 5f;
        private static float _height = 35f;
        private static int _menu = 0;
        public static void DoSettingsWindowContents(Rect rect)
        {
            float buttonWidth = 100f;

            var buttonRect = new Rect(rect.width - _padding - buttonWidth, _padding + _height, buttonWidth, 20f);
            if (Widgets.ButtonText(buttonRect, "Main"))
            {
                _menu = 0;
            }

            buttonRect.y += _height;
            if (Widgets.ButtonText(buttonRect, "Coins"))
            {
                _menu = 3;
            }

            buttonRect.y += _height;
            if (Widgets.ButtonText(buttonRect, "Products"))
            {
                _menu = 4;
            }

            buttonRect.y += _height;
            if (Widgets.ButtonText(buttonRect, "Items"))
            {
                _menu = 1;
            }

            rect.width -= buttonWidth + _padding;
            switch (_menu)
            {
                case 0:
                default:
                    MainMenu(rect);
                    break;
                case 1:
                    ItemMenu(rect);
                    break;
                case 3:
                    CoinMenu(rect);
                    break;
                case 4:
                    EventMenu(rect);
                    break;
            }
        }

        private static void MainMenu(Rect rect)
        {
            var labelRect = new Rect(_padding, _padding + _height, rect.width - (_padding * 2), rect.height - (_padding * 2));
            var inputRect = new Rect(_padding + 140f, _padding + _height, rect.width - (_padding * 2) - 140f, 20f);
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Label(labelRect, "TwitchStoriesSettingsTwitchChannel".Translate() + ": ");
            Channel = Widgets.TextField(inputRect, Channel, 999, new Regex("^[a-z0-9_]*$", RegexOptions.IgnoreCase));

            labelRect.y += _height;
            inputRect.y += _height;
            Widgets.Label(labelRect, "TwitchStoriesSettingsTwitchUsername".Translate() + ": ");
            Username = Widgets.TextField(inputRect, Username, 999, new Regex("^[a-z0-9_]*$", RegexOptions.IgnoreCase));

            labelRect.y += _height;
            inputRect.y += _height;
            Widgets.Label(labelRect, "TwitchStoriesSettingsOAuth".Translate() + ": ");
            if (_showOAuth > 2)
            {
                OAuth = Widgets.TextField(inputRect, OAuth, 999, new Regex("^[a-z0-9:]*$", RegexOptions.IgnoreCase));
            }
            else
            {
                if (Widgets.ButtonText(inputRect, ("TwitchStoriesSettingsOAuthWarning" + _showOAuth).Translate()))
                {
                    _showOAuth++;
                }
            }

            labelRect.y += _height;
            inputRect.y += _height;
            Widgets.Label(labelRect, "TwitchStoriesSettingsTwitchAutoConnect".Translate() + ": ");
            if (Widgets.ButtonText(inputRect, (AutoConnect ? "TwitchStoriesEnabled".Translate() : "TwitchStoriesDisabled".Translate())))
            {
                AutoConnect = !AutoConnect;
            }

            labelRect.y += _height;
            inputRect.y += _height;
            Widgets.Label(labelRect, "TwitchStoriesSettingsVoteTime".Translate() + ": ");
            VoteTime = (int)Widgets.HorizontalSlider(inputRect, VoteTime, 1, 15, false, VoteTime.ToString() + " " + "TwitchStoriesMinutes".Translate(), null, null, 1);

            labelRect.y += _height;
            inputRect.y += _height;
            Widgets.Label(labelRect, "TwitchStoriesSettingsVoteOptions".Translate() + ": ");
            VoteOptions = (int)Widgets.HorizontalSlider(inputRect, VoteOptions, 1, 5, false, VoteOptions.ToString() + " " + "TwitchStoriesOptions".Translate(), null, null, 1);

            labelRect.y += _height;
            inputRect.y += _height;
            inputRect.width = ((inputRect.width - _padding) / 2);
            Widgets.Label(labelRect, "TwitchStoriesSettingsOtherCommands".Translate() + ": ");
            if (Widgets.ButtonText(inputRect, "!mods " + (CommandsModsEnabled ? "TwitchStoriesEnabled".Translate() : "TwitchStoriesDisabled".Translate())))
            {
                CommandsModsEnabled = !CommandsModsEnabled;
            }

            inputRect.x += inputRect.width + _padding;
            if (Widgets.ButtonText(inputRect, "!alive " + (CommandsAliveEnabled ? "TwitchStoriesEnabled".Translate() : "TwitchStoriesDisabled".Translate())))
            {
                CommandsAliveEnabled = !CommandsAliveEnabled;
            }

            var mod = LoadedModManager.GetMod<TwitchToolkit>();
            labelRect.y += _height;
            labelRect.height = 30f;
            labelRect.width = 100f;
            if (Widgets.ButtonText(labelRect, "TwitchStoriesReconnect".Translate()))
            {
                mod.Reconnect();
            }

            labelRect.x += 100f + _padding;
            if (Widgets.ButtonText(labelRect, "TwitchStoriesDisconnect".Translate()))
            {
                mod.Disconnect();
            }

            labelRect.x = _padding;
            labelRect.y += _height;
            labelRect.height = rect.height - labelRect.y;
            labelRect.width = rect.width - (_padding * 2);
            Widgets.TextArea(labelRect, string.Join("\r\n", mod.MessageLog), true);
        }

        private static void CoinMenu(Rect rect)
        {
            Listing_TwitchToolkit listingStandard = new Listing_TwitchToolkit();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Reward Coins:", ref Settings.EarningCoins, "Should viewers earn coins while watching?");
            listingStandard.CheckboxLabeled("Store Open:", ref Settings.StoreOpen, "Enable purchasing of events and items");
            listingStandard.Label("Coins Per Interval: " + Settings.CoinAmount);
            Settings.CoinAmount = listingStandard.Slider((float)Settings.CoinAmount, 1, 50);

            listingStandard.End();
        }

        public static int ProductScroll = 0;
        private static string searchquery;

        public static int ResetViewerStage { get; private set; }
        public static string ResetViewerWarning { get; private set; }
        public static int ItemScroll { get; private set; }
        public static int ResetItemStage { get; private set; }

        private static void EventMenu(Rect rect)
        {
            var labelRect = new Rect(_padding, _padding + _height, rect.width - (_padding * 2), rect.height - (_padding * 2));
            var inputRect = new Rect(_padding + 140f, _padding + _height, rect.width - (_padding * 2) - 140f, 20f);

            if (ProductScroll > 0)
            {
                inputRect.x = _padding + (rect.width - (_padding * 2)) / 4;
                inputRect.width = (rect.width - (_padding * 2)) / 4;
                if (Widgets.ButtonText(inputRect, "^"))
                {
                    ProductScroll = Math.Max(0, ProductScroll - 3);
                }
            }

            int count = 0;
            int scroll = 0;

            if (ProductScroll < (products.Count - count))
            {
                inputRect.x = _padding + (rect.width - (_padding * 2)) / 2;
                inputRect.width = (rect.width - (_padding * 2)) / 4;
                if (Widgets.ButtonText(inputRect, "v"))
                {
                    ProductScroll += 3;
                }
            }

            inputRect.x = _padding + (rect.width - (_padding * 2)) / 2 + (rect.width - (_padding * 2)) / 4;
            inputRect.width = (rect.width - (_padding * 2)) / 4;

            if (ResetViewerStage == 0)
            {
                ResetViewerWarning = "Reset to Default";
            }
            else if (ResetViewerStage == 1)
            {
                ResetViewerWarning = "Are you sure?";
            }
            else if (ResetViewerStage == 2)
            {
                ResetViewerWarning = "One more time";
            }
            else if (ResetViewerStage == 3)
            {
                ResetViewerStage = 0;
                ResetProductData();
                EventMenu(rect);
            }

            if (Widgets.ButtonText(inputRect, ResetViewerWarning))
            {
                ResetViewerStage += 1;
            }

            inputRect.y += _height;

            Listing_TwitchToolkit listingStandard = new Listing_TwitchToolkit();
            listingStandard.Begin(rect);

            labelRect.y += _height;
            searchquery = Widgets.TextField(inputRect, searchquery, 999, new Regex("^[a-z0-9_]*$", RegexOptions.IgnoreCase));
            
            List<Product> query = products.Where(a => (a.abr.Contains(searchquery))).ToList();

            inputRect.y += _height;
            foreach (Product product in query)
            {
                if (++scroll <= ProductScroll)
                {
                    continue;
                }

                listingStandard.Label($"{product.name}: {product.amount}");
                int newprice = product.amount;

                Rect productline = listingStandard.GetRect(24f);

                productline.width = 40f;
                if (Widgets.ButtonText(productline, "-" + 500))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 500 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 50)
                    {
                        newprice = 50;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "-" + 50))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 50 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 50)
                    {
                        newprice = 50;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "-" + 10))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 10 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 50)
                    {
                        newprice = 50;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 10))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 10 * GenUI.CurrentAdjustmentMultiplier();
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 50))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 50 * GenUI.CurrentAdjustmentMultiplier();
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 500))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 500 * GenUI.CurrentAdjustmentMultiplier();
                }
                string karmabutton = "";

                if (product.karmatype == 0)
                {
                    karmabutton = "Bad";
                }
                else if (product.karmatype == 1)
                {
                    karmabutton = "Good";
                }
                else if (product.karmatype == 2)
                {
                    karmabutton = "Neutral";
                }
                else
                {
                    karmabutton = "Doom";
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, karmabutton))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    if (product.karmatype == 3)
                    {
                        product.karmatype = 0;
                        Settings.ProductKarmaTypes[product.id] = 0;
                    }
                    else
                    {
                        product.karmatype = product.karmatype + 1;
                        Settings.ProductKarmaTypes[product.id] = Settings.ProductKarmaTypes[product.id] + 1;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "Disable"))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice = -1;
                }

                listingStandard.Gap(listingStandard.verticalSpacing);

                ProductAmounts[product.id] = newprice;
                product.amount = newprice;

                count++;
            }
        }


        private static void ItemMenu(Rect rect)
        {
            var labelRect = new Rect(_padding, _padding + _height, rect.width - (_padding * 2), rect.height - (_padding * 2));
            var inputRect = new Rect(_padding + 140f, _padding + _height, rect.width - (_padding * 2) - 140f, 20f);

            if (ItemScroll > 0)
            {
                inputRect.x = _padding + (rect.width - (_padding * 2)) / 4;
                inputRect.width = (rect.width - (_padding * 2)) / 4;
                if (Widgets.ButtonText(inputRect, "^"))
                {
                    ItemScroll = Math.Max(0, ItemScroll - 3);
                }
            }

            int count = 0;
            int scroll = 0;

            if (ItemScroll < (products.Count - count))
            {
                inputRect.x = _padding + (rect.width - (_padding * 2)) / 2;
                inputRect.width = (rect.width - (_padding * 2)) / 4;
                if (Widgets.ButtonText(inputRect, "v"))
                {
                    ItemScroll += 3;
                }
            }

            inputRect.x = _padding + (rect.width - (_padding * 2)) / 2 + (rect.width - (_padding * 2)) / 4;
            inputRect.width = (rect.width - (_padding * 2)) / 4;

            if (ResetItemStage == 0)
            {
                ResetViewerWarning = "Reset to Default";
            }
            else if (ResetItemStage == 1)
            {
                ResetViewerWarning = "Are you sure?";
            }
            else if (ResetItemStage == 2)
            {
                ResetViewerWarning = "One more time";
            }
            else if (ResetItemStage == 3)
            {
                ResetItemStage = 0;
                ResetItemData();
                ItemMenu(rect);
            }

            if (Widgets.ButtonText(inputRect, ResetViewerWarning))
            {
                ResetItemStage += 1;
            }

            inputRect.y += _height;

            Listing_TwitchToolkit listingStandard = new Listing_TwitchToolkit();
            listingStandard.Begin(rect);

            labelRect.y += _height;
            searchquery = Widgets.TextField(inputRect, searchquery, 999, new Regex("^[a-z0-9_]*$", RegexOptions.IgnoreCase));
            
            List<Item> query = items.Where(a => (a.abr.Contains(searchquery))).ToList();

            inputRect.y += _height;
            foreach (Item item in query)
            {
                if (++scroll <= ItemScroll)
                {
                    continue;
                }

                listingStandard.Label($"{item.abr}: {item.price}");
                int newprice = item.price;

                Rect productline = listingStandard.GetRect(24f);

                productline.width = 40f;
                if (Widgets.ButtonText(productline, "-" + 100))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 100 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 1)
                    {
                        newprice = 1;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "-" + 10))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 10 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 1)
                    {
                        newprice = 1;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "-" + 1))
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    newprice -= 1 * GenUI.CurrentAdjustmentMultiplier();
                    if (newprice < 1)
                    {
                        newprice = 1;
                    }
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 1))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 1 * GenUI.CurrentAdjustmentMultiplier();
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 10))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 10 * GenUI.CurrentAdjustmentMultiplier();
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "+" + 100))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice += 100 * GenUI.CurrentAdjustmentMultiplier();
                }

                productline.x += productline.width + 2f;
                if (Widgets.ButtonText(productline, "Disable"))
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    newprice = -1;
                }

                listingStandard.Gap(listingStandard.verticalSpacing);

                ItemPrices[item.id] = newprice;
                item.price = newprice;

                count++;
            }
        }

        public static void ResetProductData()
        {
            products = new List<Product>();
            ProductIds = new Dictionary<string, int>();
            ProductTypes = new Dictionary<int, int>();
            ProductNames = new Dictionary<int, string>();
            ProductKarmaTypes = new Dictionary<int, int>();
            ProductAmounts = new Dictionary<int, int>();
            ProductEventIds = new Dictionary<int, int>();
            // if no previous save data create new products
            List<Product> defaultProducts = Products.GenerateDefaultProducts().ToList();
            foreach (Product product in defaultProducts)
            {
                int id = product.id;
                ProductIds.Add(product.abr, id);
                ProductTypes.Add(id, product.type);
                ProductNames.Add(id, product.name);
                ProductKarmaTypes.Add(id, product.karmatype);
                ProductAmounts.Add(id, product.amount);
                ProductEventIds.Add(id, product.evtId);
                products.Add(product);
            }
        }

        public static void ResetItemData()
        {
            items = new List<Item>();
            ItemIds = new Dictionary<string, int>();
            ItemPrices = new Dictionary<int, int>();
            ItemDefnames = new Dictionary<int, string>();
            ItemStuffnames = new Dictionary<int, string>();

            List<Item> defaultitems = Item.GetDefaultItems().ToList();

            foreach(Item item in defaultitems)
            {
                int id = item.id;
                ItemIds.Add(item.abr, id);
                ItemPrices.Add(id, item.price);
                ItemDefnames.Add(id, item.defname);
                if (item.stuffname != "null")
                {
                    ItemStuffnames[id] = item.stuffname;
                }
                else
                {
                    ItemStuffnames[id] = "null";
                }
                items.Add(item);
            }
        }

    }
}