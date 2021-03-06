﻿using VRageMath;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.ModAPI;
using System.IO;
using Sandbox.Game.Entities;
using Sandbox.Game;
using Sandbox.Game.Definitions;
using Sandbox.Game.EntityComponents;
using VRage.Utils;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Library;
using VRage;
using Sandbox.Game.World;
using Sandbox.Definitions;
using SpaceEngineers.ObjectBuilders;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.Game.ObjectBuilders;
using Sandbox.Common.ObjectBuilders;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities.Character.Components;
using VRage.Game.ModAPI.Interfaces;
using System.Timers;
using Sandbox.ModAPI.Interfaces;
using VRage.Collections;
using Sandbox.Game.Gui;
using Sandbox.ModAPI.Interfaces.Terminal;
using ProtoBuf;
using SpaceEngineers.Game.ModAPI;

/*
 ───█───▄▀█▀▀█▀▄▄───▐█──────▄▀█▀▀█▀▄▄
──█───▀─▐▌──▐▌─▀▀──▐█─────▀─▐▌──▐▌─█▀
─▐▌──────▀▄▄▀──────▐█▄▄──────▀▄▄▀──▐▌
─█────────────────────▀█────────────█
▐█─────────────────────█▌───────────█
▐█─────────────────────█▌───────────█
─█───────────────█▄───▄█────────────█
─▐▌───────────────▀███▀────────────▐▌
──█──────────▀▄───────────▄▀───────█
───█───────────▀▄▄▄▄▄▄▄▄▄▀────────█ 
devBranch
 */

namespace ShipCoreMainBlock
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), true, "ShipCoreMainBlock")]
    public class ShipCore : MyGameLogicComponent
    {
        private bool _init = false;

        public static AllLimits MyLimitsSettings;
        public static List<Vector3I> addonsPositions;
        public static List<Vector3I> addonsPositionstest;
        private Color COLOROFF = new Color(240, 240, 240);
        private Color GREEN = new Color(0, 240, 0);
        private Color YELLOW = new Color(240, 150, 0);
        private Color RED = new Color(240, 0, 0);
        private Color PURPLE = new Color(20, 150, 240);
        private MyEntity3DSoundEmitter LOOP_soundEmitter;

        public MyEntity3DSoundEmitter LoopSoundEmitter { get { return LOOP_soundEmitter; } }
        float currentBlocks = 0.0f;
        bool _processing = false;
        bool _processing2 = false;
        bool hasoverhead = false;
        bool hasoverheadblocks = false;
        bool addonschanged = true;
        MyEntity m_display;
        MyCubeGrid m_mycubegrid;
        IMyCubeBlock m_block;
        private static List<IMyTerminalControlLabel> MyLabelCoreList = new List<IMyTerminalControlLabel>();
        bool? isWorking = null;
        bool inited = false;
        private bool[] myflag = { false, false, false, false, false };
        public static IMyBatteryBlock mycore;
        bool debug = true;
        //public static MyStringHash add4 = MyStringHash.GetOrCompute("ShipCore_Add04");
         
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            ShowMessageInGame("dbg", "GetObjectBuilder");
            return Container.Entity.GetObjectBuilder(copy);

        }

        /// <summary>
        /// инициализация блока
        /// </summary>
        /// <param name="objectBuilder"></param>
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {

            //MyAPIGateway.Session.Config.Language
            //   if (MyAPIGateway.Session == null)
            //       return;

            //CLIENT only please
            //  if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer)
            //     return;

            if (Entity.Physics == null || IsProjectable((IMyCubeBlock)Entity)) return;

            var a = ((IMyCubeBlock)Entity).GetUserRelationToOwner(MyAPIGateway.Session.Player.IdentityId);
            bool b = a.IsFriendly();
            ShowMessageInGame("Init ", " IsFriendly " + b);
            if (!b)
            {
                NeedsUpdate |= MyEntityUpdateEnum.NONE;
                Close();
                return;
            }

            //    (((IMyCubeBlock)Entity) as IMyTerminalBlock).OwnershipChanged += )

            ShowMessageInGame("dbg", "init!");
            base.Init(objectBuilder);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;  //for draw

            

        }
        /// <summary>        /// 
        /// делаем грязь каждые 100 тиков
        /// </summary>
        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            var a = ((IMyCubeBlock)Entity).GetUserRelationToOwner(MyAPIGateway.Session.Player.IdentityId);
            bool b = a.IsFriendly();
            ShowMessageInGame("UpdateAfterSimulation100 ", " IsFriendly " + b);
            if (!b)
            {
                NeedsUpdate = MyEntityUpdateEnum.NONE;
                Close();
                return;
            }
            if (!_init) MyInit(); //all init
            try
            {
                // mycore.RefreshCustomInfo();
                if (m_display == null)
                {
                    ShowMessageInGame("dbg", "if (m_display == null) start");
                    // ShowMessageInGame("dbg", "m_display = m_block as MyEntity");
                    m_display = m_block as MyEntity; //  m_display = LoadDisplay();
                    m_display.SetEmissiveParts("Em_ONOFF", GREEN, 1f);
                    OnblockEvent(m_block as IMySlimBlock);
                    OnblockEvent(m_block as IMySlimBlock);
                    TryUpdateBlock();
                    ShowMessageInGame("dbg", "if (m_display == null) end");
                }
                if ((m_block.CubeGrid as MyCubeGrid != m_mycubegrid) || ((m_block.CubeGrid as MyCubeGrid).BlocksCount <= 1))
                {
                    m_display.SetEmissiveParts("Em_ONOFF", RED, 1f);
                    NeedsUpdate |= MyEntityUpdateEnum.NONE;
                    m_display = null;
                    m_block = null;

                    ShowMessageInGame("dbg", "try Block Close()");
                    Close();
                    m_mycubegrid = null;

                }
                (m_block as IMyTerminalBlock).ShowOnHUD = true;
                TryUpdateBlock();
                NeedsUpdate |= MyEntityUpdateEnum.NONE;
                DetectAddons();
                addonschanged = true;
                OnblockEvent(m_block as IMySlimBlock);
              

            }
            catch
            {
                ShowMessageInGame("dbg", "UpdateAfterSimulation100 catch");
            }
        }

        private void CheckAndReplaceOwner()
        {
            ShowMessageInGame("dbg", "m_mycubegrid.BigOwners " + m_mycubegrid.BigOwners.Count + " small" + m_mycubegrid.SmallOwners.Count);


            if (m_mycubegrid.BigOwners.Count > 1 || m_mycubegrid.SmallOwners.Count > 1)
            {
                m_mycubegrid.ChangeGridOwner(m_block.OwnerId, MyOwnershipShareModeEnum.Faction);

            }
            

               
            
        }

        /// <summary>
        /// инициализация логики нашего мода
        /// </summary>
        private void MyInit()
        {

            Log.init("debug.log");
            Log.writeLine("<CoreBlock> Logging started.");
            ShowMessageInGame("dbg", "first init");
            SetUpDefaultLimits();
            m_block = (IMyCubeBlock)Entity;
            m_block.NeedsWorldMatrix = true;

            (m_block as IMyTerminalBlock).CustomName = "Core Status: Loading";

            (m_block as IMyTerminalBlock).ShowOnHUD = true;

            m_mycubegrid = m_block.CubeGrid as MyCubeGrid;

            // m_grid = m_block.CubeGrid;
            m_mycubegrid.OnBlockAdded += OnblockEvent;
            m_mycubegrid.OnBlockRemoved += OnblockEvent;
           
            //   m_block.IsWorkingChanged
            LOOP_soundEmitter = new MyEntity3DSoundEmitter(Container.Entity as VRage.Game.Entity.MyEntity);
            ShowMessageInGame("dbg", "init end!");
            Vector3I corepos = m_block.Position;
            ShowMessageInGame("dbgcorepos", corepos.ToString());

            Matrix mmatrix;
            m_block.Orientation.GetMatrix(out mmatrix);
            addonsPositions = new List<Vector3I>()//слева справа сверху снизу ссади
            {
                   new Vector3I(corepos+mmatrix.Left),//слева
                 new Vector3I(corepos+mmatrix.Right), //справа
                 new Vector3I(corepos+mmatrix.Up), //сверху
                new Vector3I(corepos+mmatrix.Down), //снизу
                new Vector3I(corepos+mmatrix.Backward), //ссади
            };





            string tmp = "";
            foreach (var str in addonsPositions)
            {
                tmp += str.ToString();

            }
            ShowMessageInGame("dbg", tmp);



            DetectAddons();

            (m_block as IMyTerminalBlock).ShowOnHUD = true;
            _init = true;

            MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControls_CustomControlGetter;
            mycore = Container.Entity as IMyBatteryBlock;
            mycore.AppendingCustomInfo += Tool_AppendingCustomInfo;
            CheckAndReplaceOwner();
            CreateWeirdLabels();

        }

        private void CreateWeirdLabels()
        {
           

            for (var i = 0; i <= 6; i++) { 
            var MyLabelCore = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBatteryBlock>("kek"+i);
            MyLabelCore.Label = MyStringId.GetOrCompute(">No info.");
            MyLabelCore.Visible = IsMyBlock;
            MyAPIGateway.TerminalControls.AddControl<IMyBatteryBlock>(MyLabelCore);
            MyLabelCoreList.Add(MyLabelCore);

            }
            ShowMessageInGame("CreateWeirdLabels ", MyLabelCoreList.Count.ToString());
        }

        private static bool IsMyBlock(IMyTerminalBlock Block)
        {
            if (Block.BlockDefinition.SubtypeId == "ShipCoreMainBlock")
            {
                return true;
            }
            return false;
        }
        private void Tool_AppendingCustomInfo(IMyTerminalBlock trash, StringBuilder Info)
        {
            Info.Clear();
            Info.AppendLine($">CORE STATUS:<");

            if (MyLimitsSettings != null && MyLimitsSettings?.installedAddons?.Count > 0)
            {
                Info.AppendLine($">Addons in use: " + MyLimitsSettings.installedAddons.Count);
            }
            else
            {
                Info.AppendLine($">No addons installed.");
            }

        }
        private void TerminalControls_CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block.BlockDefinition.SubtypeId == "ShipCoreMainBlock")
            {

                foreach (var control in controls.ToList())
                {

                    if (control.Id == "ShowInInventory" || control.Id == "OnOff" || control.Id == "ShowInToolbarConfig" || control.Id == "Name" || control.Id == "CustomData")
                        controls.Remove(control);

                }


               
                
                if (MyLimitsSettings.installedAddons?.Count > 0)
                {


                    string[] tmp = new string[6];
                    tmp[0] = ">Addons connected list: " + MyLimitsSettings.installedAddons.Count;
                    int i = 1;
                    foreach (var addon in MyLimitsSettings.installedAddons)
                    {
                        tmp[i] = ")>" + addon.ToString() ;
                        tmp[i].PadRight(35- tmp[i].Length, ' ');
                        ShowMessageInGame("CCGetter ", "in foreach" + addon.ToString());
                        i++;
                    }


                    ShowMessageInGame("CCGetter ", "tmp : " + tmp + "  GetOrCompute: " + tmp.Length);
                    WriteToLWeirdLabels(tmp,i);

                }
            }

        }

        private void WriteToLWeirdLabels(string[] tmp,int count)
        {
            int  i = 0;
            for (i=0; i <= count-1; i++)
            {

                MyLabelCoreList[i].Label = (MyStringId)MyStringId.GetOrCompute(tmp[i]);
                ShowMessageInGame("CCGetter ", "ok end");
                MyLabelCoreList[i].RedrawControl();
                MyLabelCoreList[i].UpdateVisual();
                ShowMessageInGame("WriteToLWeirdLabels  ",MyLabelCoreList[i].Id + MyLabelCoreList[i].Label.String);
            }            
            for ( i=i; i <=6; i++)
            {
                MyLabelCoreList[i].Label = MyStringId.GetOrCompute(""); ;
                ShowMessageInGame("CCGetter2 ", "ok end");
                MyLabelCoreList[i].RedrawControl();
                MyLabelCoreList[i].UpdateVisual();
                ShowMessageInGame("WriteToLWeirdLabels  ", MyLabelCoreList[i].Id + MyLabelCoreList[i].Label.String);
            }



               
        }

        private void TryUpdateBlockLimits()
        {
            ShowMessageInGame("dbg", "TryUpdateBlockLimits start");
            try
            {
                if (m_mycubegrid == null) return;
                var entity = m_mycubegrid;
                if (!(entity is IMyCubeGrid))
                    return;

                if (entity.Physics == null || entity.MarkedForClose || entity.Closed)
                    return;

                if (!entity.InScene)
                    return;
                bool temphasoverhead = false;
                //  ShowMessageInGame("dbg", "TryUpdateBlockLimits point1");
                if (addonschanged) { UpdateAddons(); }
                CheckAndReplaceOwner();
                //Dictionary<AllLimits.Addons, int> copy_installedAddons = new Dictionary<AllLimits.Addons, int>(MyLimitsSettings.installedAddons);
                Dictionary<AllLimits.BlockLimitItem, int> blocks = new Dictionary<AllLimits.BlockLimitItem, int>();
                IMyCubeGrid grid = (IMyCubeGrid)entity;
                List<IMySlimBlock> blockstoProcess = new List<IMySlimBlock>();
                grid.GetBlocks(blockstoProcess, x => x.FatBlock is IMyTerminalBlock);
                foreach (IMySlimBlock myTerminalBlock in blockstoProcess)
                {
                    if (myTerminalBlock?.FatBlock == null)
                        continue;

                    IMyTerminalBlock block = (IMyTerminalBlock)myTerminalBlock.FatBlock;
                    foreach (AllLimits.BlockLimitItem item in MyLimitsSettings.Big_List_of_Limits)
                    {
                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.Off)
                            continue;

                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.BlockTypeId && string.IsNullOrEmpty(item.BlockTypeId))
                        {

                            ShowMessageInGame("dbg", "Block Enforcement item for \"{0}\" is set to mode BlockTypeId but does not have BlockTypeId set.");
                            continue;
                        }
                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.BlockSubtypeId && string.IsNullOrEmpty(item.BlockSubtypeId))
                        {
                            ShowMessageInGame("dbg", "Block Enforcement item for \"{0}\" is set to mode BlockSubtypeId but does not have BlockSubtypeId set.");
                            continue;
                        }

                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.BlockSubtypeId
                            && !string.IsNullOrEmpty(block.BlockDefinition.SubtypeId)
                            && block.BlockDefinition.SubtypeId.Contains(item.BlockSubtypeId))
                        {
                            if (blocks.ContainsKey(item))
                                blocks[item] += 1;
                            else
                                blocks.Add(item, 1);
                        }

                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.BlockTypeId
                            && !string.IsNullOrEmpty(block.BlockDefinition.TypeIdString)
                            && block.BlockDefinition.TypeIdString.Contains(item.BlockTypeId))
                        {
                            if (blocks.ContainsKey(item))
                                blocks[item] += 1;
                            else
                                blocks.Add(item, 1);
                        }
                    }
                    foreach (AllLimits.BlockLimitItem item in MyLimitsSettings.Big_List_of_Limits)
                    {
                        if (item.Mode == AllLimits.BlockLimitItem.EnforcementMode.Off)
                            continue;

                        if (!blocks.ContainsKey(item))
                            continue;

                        if (blocks[item] > item.MaxPerGrid)
                        {
                            // if (!MyAPIGateway.Session.HasCreativeRights)
                            // {

                            if (MyAPIGateway.Session.Config.Language == MyLanguagesEnum.Russian)
                            {
                                ShowMessageInGame("ShipCore:", string.Format("Вы превысили максимальное количество блоков {0} в корабле '{1}'.  При следующей чистке удаляться {2} блока!", item.BlockSubtypeId, grid.DisplayName, blocks[item] - item.MaxPerGrid));
                            }
                            ShowMessageInGame("ShipCore:", string.Format("You have exceeded the max block count of {0} on the ship '{1}'. Next cleanup will delete {2} block(s) to enforce this block limit.", item.BlockSubtypeId, grid.DisplayName, blocks[item] - item.MaxPerGrid));
                            temphasoverhead = true;
                            //  }
                        }
                    }



                }
                // ShowMessageInGame("dbg", "hasoverhead" + hasoverhead + "temphasoverhead" + temphasoverhead);
                hasoverhead = temphasoverhead;

                if (!(m_block.Closed || m_block.MarkedForClose))
                {
                    SetBlockState();
                    _processing2 = false;
                }
                else
                {
                    _processing2 = false;
                    //ShowMessageInGame("dbg", "hasoverhead null found!");
                }
                ShowMessageInGame("dbg", "TryUpdateBlockLimits end");
            }
            catch
            {
                // ShowMessageInGame("dbg", "TryUpdateBlockLimits catch!");
                _processing2 = false;
            }
            finally
            {
                _processing2 = false;

            }

        }

        public void OnblockEvent(IMySlimBlock block)
        {  //ShowMessageInGame("dbg", "OnblockEvent!");
            if (block == null) return;
            if (m_block == null) return;
            if (m_mycubegrid == null) return;
            if (m_display == null) return;

            //  ShowMessageInGame("dbg", "OnblockEvent!" + (m_block as IMyCubeBlock).Position.ToString() + (block as IMyCubeBlock)?.Position.ToString());

            if (addonsPositions.Contains(block.Position))
            {
                DetectAddons();
                ShowMessageInGame("dbg", "DetectAddons!");
            }

            TryUpdateBlock();
            CheckAndReplaceOwner();
            if (_processing2) //worker thread 2 is busy
                return;

            _processing2 = true;

            MyAPIGateway.Parallel.Start(TryUpdateBlockLimits);


        }

        public override void Close()
        {
            Log.close();

            ShowMessageInGame("dbg", "Block Close()");
            if (m_mycubegrid != null)
            {
                m_mycubegrid.OnBlockAdded -= OnblockEvent;
                m_mycubegrid.OnBlockRemoved -= OnblockEvent;
                MyAPIGateway.TerminalControls.CustomControlGetter -= TerminalControls_CustomControlGetter;
                mycore.AppendingCustomInfo -= Tool_AppendingCustomInfo;
                for (var i = 0; i <= 6; i++)
                {
                    MyAPIGateway.TerminalControls.RemoveControl<IMyBatteryBlock>(MyLabelCoreList[i]);
                }
            }


        }

        /// <summary>
        /// Сброс лимитов и перезаргрузка с учетом изменившихся аддонов
        /// </summary>
        private void UpdateAddons()
        {
            
            ShowMessageInGame("dbg", "UpdateAddons start");
            addonschanged = false;
            SetUpDefaultLimits();// слева справа сверху снизу ссади


            if (m_mycubegrid.CubeExists(addonsPositions[3]) &&
                ((m_mycubegrid.GetCubeBlock(addonsPositions[3]) as IMySlimBlock).BlockDefinition.Id.SubtypeId == AllLimits.namelistaddonsreverse[AllLimits.Addons.Сooler]))
            {
                var a = m_mycubegrid.GetCubeBlock(addonsPositions[3]) as IMySlimBlock;

                ShowMessageInGame("dbg", "UpdateAddons ShipCore_Add04" 
                    + (m_mycubegrid.CubeExists(addonsPositions[3]).ToString() 
                    + a.BlockDefinition.Id.SubtypeId.String) + (m_mycubegrid.GetCubeBlock(addonsPositions[3]) as IMySlimBlock).IsFullIntegrity);

                if (MyLimitsSettings.installedAddons.ContainsKey(AllLimits.Addons.Сooler))
                {
                    MyLimitsSettings.installedAddons[AllLimits.Addons.Сooler]++;
                    ShowMessageInGame("dbg", "UpdateAddons installedAddons +1 Сooler" + MyLimitsSettings.installedAddons.Count);
                }

                else
                {
                    MyLimitsSettings.installedAddons.Add(AllLimits.Addons.Сooler, 1);
                }


            }  //снизу .Охладитель


            if (m_mycubegrid.CubeExists(addonsPositions[0]))//слева
            {
                if ((m_mycubegrid.GetCubeBlock(addonsPositions[0]) as IMySlimBlock).IsFullIntegrity)
                {
                    LoadAddon(m_mycubegrid.GetCubeBlock(addonsPositions[0]));

                }
            }
            if (m_mycubegrid.CubeExists(addonsPositions[1]) && (m_mycubegrid.GetCubeBlock(addonsPositions[1]) as IMySlimBlock).IsFullIntegrity)//справа
            {
                LoadAddon(m_mycubegrid.GetCubeBlock(addonsPositions[1]));
            }
            if (m_mycubegrid.CubeExists(addonsPositions[2]) && (m_mycubegrid.GetCubeBlock(addonsPositions[2]) as IMySlimBlock).IsFullIntegrity)//сверху
            {
                LoadAddon(m_mycubegrid.GetCubeBlock(addonsPositions[2]));
            }

            if (m_mycubegrid.CubeExists(addonsPositions[4]) && (m_mycubegrid.GetCubeBlock(addonsPositions[4]) as IMySlimBlock).IsFullIntegrity)//ссади
            {
                LoadAddon(m_mycubegrid.GetCubeBlock(addonsPositions[4]));
            }


            ShowMessageInGame("dbg", "UpdateAddons end");


        }
        /// <summary>
        /// загрузка информации об аддонах
        /// </summary>
        /// <param name="block"></param>
        private void LoadAddon(IMySlimBlock block)
        {
           
            ShowMessageInGame("dbg", "LoadAddon start");

            MyStringHash subtype = block.BlockDefinition.Id.SubtypeId;
            ShowMessageInGame("dbg", "LoadAddon MyStringHash: "+ subtype.String);

            AllLimits.Addons TmpAddon = AllLimits.Addons.Сooler;
            if (AllLimits.namelistaddons.ContainsKey(subtype) && (subtype != AllLimits.namelistaddonsreverse[AllLimits.Addons.Сooler]))
                TmpAddon = AllLimits.namelistaddons[subtype];  // AllLimits.Addons.friend_or_foe_transponder;
            else
            {
                ShowMessageInGame("dbg", " wrong block addon" + (subtype == AllLimits.namelistaddonsreverse[AllLimits.Addons.Сooler]));
                return;
            }

            if (MyLimitsSettings.installedAddons.ContainsKey(TmpAddon))
            {
                MyLimitsSettings.installedAddons[TmpAddon]++;
                
            }
            else
            {
                MyLimitsSettings.installedAddons.Add(TmpAddon, 1);
                ShowMessageInGame("dbg", "add in LoadAddon");
                ShowMessageInGame("dbg", "add in LoadAddon. installedAddons: " + MyLimitsSettings.installedAddons.Count);
            }
            ShowMessageInGame("dbg", "LoadAddon end");
        }
        /// <summary>
        /// проверям только нужные нам грани //оптимизировать над потом
        /// </summary>
        private void DetectAddons()
        {

            bool[] myflag1 = new bool[] { false, false, false, false, false };

            if (m_mycubegrid.CubeExists(addonsPositions[0]) && (m_mycubegrid.GetCubeBlock(addonsPositions[0]) as IMySlimBlock).IsFullIntegrity)
            {

                myflag1[0] = true;
            }
            if (m_mycubegrid.CubeExists(addonsPositions[1]) && (m_mycubegrid.GetCubeBlock(addonsPositions[1]) as IMySlimBlock).IsFullIntegrity)
            {
                myflag1[1] = true;
            }
            if (m_mycubegrid.CubeExists(addonsPositions[2]) && (m_mycubegrid.GetCubeBlock(addonsPositions[2]) as IMySlimBlock).IsFullIntegrity)
            {
                myflag1[2] = true;
            }
            if (m_mycubegrid.CubeExists(addonsPositions[3]) && (m_mycubegrid.GetCubeBlock(addonsPositions[3]) as IMySlimBlock).IsFullIntegrity)
            {
                myflag1[3] = true;
            }
            if (m_mycubegrid.CubeExists(addonsPositions[4]) && (m_mycubegrid.GetCubeBlock(addonsPositions[4]) as IMySlimBlock).IsFullIntegrity)
            {
                myflag1[4] = true;
            }
            if ((myflag[0] != myflag1[0]) | (myflag[1] != myflag1[1]) | (myflag[2] != myflag1[2]) | (myflag[3] != myflag1[3]) | (myflag[4] != myflag1[4]))
            {
                addonschanged = true;
                ShowMessageInGame("dbg", " addonschanged" + addonschanged);
                myflag = myflag1;
            }
            else
            {
               // ShowMessageInGame("dbg", " addonschanged" + addonschanged);
            }

        }
        public void TryUpdateBlock()
        {
            //ShowMessageInGame("dbg", "TryUpdateBlock start!");
            try
            {
                bool isPowered = IsWorking();
                //  ShowMessageInGame("dbg", "TryUpdateBlock isPowered ! " + isPowered);
                if (isPowered)
                {
                    //  ShowMessageInGame("dbg", "TryUpdateBlock start3!");
                    UpdateBlockLevel(isPowered);
                    //  ShowMessageInGame("dbg", "TryUpdateBlock end after UpdateBlockLevel!");
                }
                else
                {
                    (m_block as IMyBatteryBlock).Enabled = true;
                    UpdateBlockLevel(true);
                }
            }
            catch
            {
                // ShowMessageInGame("dbg", "TryUpdateBlock catch!");
            }
            finally
            { //ShowMessageInGame("dbg", "TryUpdateBlock finally!"); 
            }
        }
        public void SetBlockState()
        {
            //   ShowMessageInGame("dbg", "SetBlockState: " + hasoverhead + hasoverheadblocks);
            if (hasoverhead || hasoverheadblocks)
            {

                if (!LoopSoundEmitter.IsPlaying)
                {
                    //  ShowMessageInGame("dbg", "!LoopSoundEmitter.IsPlaying!");
                    m_block.SetDamageEffect(true);
                    // MyAPIGateway.Utilities.InvokeOnGameThread(() => { m_display.SetEmissiveParts("Em_ONOFF", RED, 1f); });
                    m_display.SetEmissiveParts("Em_ONOFF", RED, 1f);
                    PlayLoopSound("Foogs.JumpDriveChargeLoop");
                    (m_block as IMyTerminalBlock).CustomName = "Core Status: ERROR";

                    (m_block as IMyTerminalBlock).ShowOnHUD = true;
                }
            }
            else
            {
                (m_block as IMyTerminalBlock).CustomName = "Core Status: Ok";


                if (LoopSoundEmitter.IsPlaying)
                {
                    // ShowMessageInGame("dbg", "hasoverhead|| hasoverheadblocks false set emmisive!");
                    m_block.SetDamageEffect(false);
                    m_display.SetEmissiveParts("Em_ONOFF", GREEN, 1f);
                    PlayLoopSound("");

                }
            }
        }

        public void UpdateBlockLevel(bool? isWorking = null)
        {
            try
            {
                if (isWorking == null || isWorking == false)
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        //   ShowMessageInGame("dbg", "UpdateBar isWorking == null || isWorking == false) ");
                        m_display.SetEmissiveParts("Em_" + i, COLOROFF, 0f);
                    }
                }
                else
                {
                    float fill = GetCurrentBlockLimitInPercent();
                    if (fill == -1f)
                    {
                        hasoverheadblocks = true;
                    }
                    else { hasoverheadblocks = false; }
                    SetBlockState();
                    // ShowMessageInGame("dbg", "UpdateBar fill =  " + fill);
                    for (int i = 1; i <= 100; i++)
                    {
                        Color c = COLOROFF;
                        float intensity = 1f;

                        if ((i / 100f) <= fill)
                        {
                            if (fill >= 0.75f && fill < 0.850f)
                                c = YELLOW;
                            else if (fill >= 0.85f && fill <= 1.00f)
                                c = RED;
                            else
                                c = GREEN;
                            m_display.SetEmissiveParts("Em_" + i, c, intensity);
                        }
                        else
                        {
                            if (fill == -1f) // overflow
                                c = RED;
                            else
                                intensity = 0.5f;

                            m_display.SetEmissiveParts("Em_" + i, c, intensity);
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void PlayLoopSound(string soundname, bool stopPrevious = false, float maxdistance = 1000, float CustomVolume = 1f, bool CanPlayLoopSounds = true)
        {
            // Logging.Instance.WriteLine($"PlayLoopSound");
            MyEntity3DSoundEmitter emitter = null;
            emitter = LoopSoundEmitter;

            if (emitter != null)
            {
                if (string.IsNullOrEmpty(soundname))
                {
                    emitter.StopSound((emitter.Loop ? true : false), true);
                }
                else
                {
                    MySoundPair sound = new MySoundPair(soundname);
                    emitter.CustomMaxDistance = maxdistance;
                    // Logger.Instance.LogDebug("Distance: " + emitter.CustomMaxDistance);
                    emitter.CustomVolume = CustomVolume;
                    emitter.SourceChannels = 2;
                    emitter.Force2D = true;
                    emitter.CanPlayLoopSounds = CanPlayLoopSounds;
                    emitter.PlaySound(sound, stopPrevious);
                }
            }
        }

        public bool IsWorking()
        {
            if (string.IsNullOrEmpty(m_mycubegrid.Name)) // if gridname is null create one
            {
                m_mycubegrid.Name = "INCORRECT_NAME_" + m_mycubegrid.EntityId.ToString();
                MyAPIGateway.Entities.SetEntityName(m_mycubegrid, true);
            }

            if (!string.IsNullOrEmpty(m_mycubegrid.Name)) // gridname is not null
            {
                return MyVisualScriptLogicProvider.HasPower(m_mycubegrid.Name) && m_block.IsWorking;
            }

            return false;
        }

        public bool IsProjectable(IMyCubeBlock Block, bool CheckPlacement = true)
        {
            MyCubeGrid Grid = Block.CubeGrid as MyCubeGrid;
            if (!CheckPlacement) return Grid.Projector != null;
            return Grid.Projector != null && (Grid.Projector as IMyProjector).CanBuild((IMySlimBlock)Block, true) == BuildCheckResult.OK;
        }

        private void ShowMessageInGame(string ot, string msg)
        {
            if (debug)
            {
                MyAPIGateway.Utilities.ShowMessage(ot, msg);
                Log.writeLine(ot + msg);
            }
        }
        /// <summary>
        /// сброс лимитов в дефолт
        /// </summary>
        private void SetUpDefaultLimits()
        {
            MyLimitsSettings = null;
            MyLimitsSettings = new AllLimits();

        }
        public float GetCurrentBlockLimitInPercent()
        {
            currentBlocks = (float)m_mycubegrid.BlocksCount;

            if (m_mycubegrid.GridSizeEnum == MyCubeSize.Large)
            {
                if (m_mycubegrid.IsStatic)
                {
                    if (currentBlocks <= MyLimitsSettings.MAX_BLOCKS_IN_STATION)
                        return (currentBlocks * 1f / MyLimitsSettings.MAX_BLOCKS_IN_STATION);
                }
                if (currentBlocks <= MyLimitsSettings.MAX_BLOCKS_IN_LARGE_SHIP)
                    return (currentBlocks * 1f / MyLimitsSettings.MAX_BLOCKS_IN_LARGE_SHIP);
            }
            else
            {
                if (currentBlocks <= MyLimitsSettings.MAX_BLOCKS_IN_SMALL_SHIP)
                    return (currentBlocks * 1f / MyLimitsSettings.MAX_BLOCKS_IN_SMALL_SHIP);
            }
            return -1f;
        }
    }
}
