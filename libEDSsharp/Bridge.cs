﻿/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2016 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Xml2CSharp;
using System.Text.RegularExpressions;

/* I know i'm going to regret this
 * 
 * I quite like my eds class as it trys to validate the EDS using typing and enums etc
 * but i also want the XML wrappers for the CanOpenXML
 * so i'm going to make a converter outside of both classes hence this bridge
 * which is more code to manage ;-(
 * */

namespace libEDSsharp
{
    public class Bridge
    {

        public Device convert(EDSsharp eds)
        {
            eds.updatePDOcount();

            Device dev = new Device();
            dev.CANopenObjectList = new CANopenObjectList();
            dev.CANopenObjectList.CANopenObject = new List<CANopenObject>();

            /* OBJECT DICTIONARY */

            foreach(KeyValuePair<UInt16,ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;

               // if(od.subindex==-1)
                {
                    CANopenObject coo = new CANopenObject();
                    coo.Index =string.Format("{0:x4}",od.index);
                    coo.Name = od.parameter_name;
                    coo.ObjectType = od.objecttype.ToString();
                    coo.Disabled = od.Disabled.ToString().ToLower();
                    coo.MemoryType = od.location.ToString();
                    coo.AccessType = od.accesstype.ToString();
                    coo.DataType = string.Format("0x{0:x2}",(int)od.datatype);
                    coo.DefaultValue = od.defaultvalue;
                    coo.PDOmapping = od.PDOtype.ToString();
                    coo.TPDOdetectCOS = od.TPDODetectCos.ToString().ToLower();
                    coo.AccessFunctionPreCode = od.AccessFunctionPreCode;
                    coo.AccessFunctionName = od.AccessFunctionName;

                    coo.Description = new Description();
                    coo.Description.Text = od.Description;
                        
                    //if (od.objecttype == ObjectType.ARRAY || od.objecttype == ObjectType.REC)
                    {
                        coo.SubNumber = od.nosubindexes.ToString(); //-1?? //check me 
                        coo.CANopenSubObject = new List<CANopenSubObject>();

                        foreach(KeyValuePair<UInt16,ODentry> kvp2 in od.subobjects)
                        {
                            ODentry subod = kvp2.Value;
                   
                            CANopenSubObject sub = new CANopenSubObject();

                            sub.Name = subod.parameter_name;
                            sub.ObjectType = subod.objecttype.ToString();
                            sub.AccessType = subod.accesstype.ToString();
                            sub.DataType = string.Format("0x{0:x2}", (int)subod.datatype);
                            sub.DefaultValue = subod.defaultvalue;
                            sub.PDOmapping = subod.PDOtype.ToString();
                            sub.SubIndex = String.Format("{0:x2}",subod.subindex);
                            sub.TPDOdetectCOS = subod.TPDODetectCos.ToString().ToLower();
                            coo.CANopenSubObject.Add(sub);
                                                 
                        }
                    }

                    if (od.objecttype == ObjectType.ARRAY && od.datatype == DataType.UNKNOWN)
                    {
                        //add the datatype field to parent objects if they don't have it already
                        //if the 2nd subobject does not exist then we do nothing.
                        ODentry sub = od.getsubobject(1);
                        if (sub != null)
                        {
                            od.datatype = sub.datatype;
                        }


                    }

                    dev.CANopenObjectList.CANopenObject.Add(coo);
                }

            }
  

            /* DUMMY USAGE */
            
            dev.Other = new Other();
            dev.Other.DummyUsage = new DummyUsage();
            dev.Other.DummyUsage.Dummy = new List<Dummy>();

            Dummy d; 

            d = new Dummy();
            d.Entry = eds.du.Dummy0001.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0002.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0003.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0004.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0005.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0006.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);
            d = new Dummy();
            d.Entry = eds.du.Dummy0007.ToString();
            dev.Other.DummyUsage.Dummy.Add(d);


            SupportedBaudRate baud = new SupportedBaudRate();
            dev.Other.BaudRate = new BaudRate();
            dev.Other.BaudRate.SupportedBaudRate = new List<SupportedBaudRate>();

            baud.Value="10 Kbps";
            if (eds.di.BaudRate_10 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "20 Kbps";
            if (eds.di.BaudRate_20 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "50 Kbps";
            if (eds.di.BaudRate_50 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "125 Kbps";
            if (eds.di.BaudRate_125 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "250 Kbps";
            if (eds.di.BaudRate_250 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "500 Kbps";
            if (eds.di.BaudRate_500 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "800 Kbps";
            if (eds.di.BaudRate_800 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);

            baud = new SupportedBaudRate();
            baud.Value = "1000 Kbps";
            if (eds.di.BaudRate_1000 == true)
                dev.Other.BaudRate.SupportedBaudRate.Add(baud);


            dev.Other.Capabilities = new Capabilities();
            dev.Other.Capabilities.CharacteristicsList = new CharacteristicsList();
            dev.Other.Capabilities.CharacteristicsList.Characteristic = new List<Characteristic>();

           
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("SimpleBootUpSlave", eds.di.SimpleBootUpSlave.ToString()));
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("SimpleBootUpMaster", eds.di.SimpleBootUpMaster.ToString()));
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("DynamicChannelsSupported", eds.di.DynamicChannelsSupported.ToString()));
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("CompactPDO", eds.di.CompactPDO.ToString()));
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("GroupMessaging", eds.di.GroupMessaging.ToString()));
            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("LSS_Supported", eds.di.LSS_Supported.ToString()));

            dev.Other.Capabilities.CharacteristicsList.Characteristic.Add(makecharcteristic("Granularity", eds.di.Granularity.ToString()));

            dev.Other.DeviceIdentity = new DeviceIdentity();
            dev.Other.DeviceIdentity.ProductName = eds.di.ProductName;
            dev.Other.DeviceIdentity.ProductNumber = eds.di.ProductNumber;
            dev.Other.DeviceIdentity.ProductText = new ProductText();
            dev.Other.DeviceIdentity.ProductText.Description = new Description();
            dev.Other.DeviceIdentity.ProductText.Description.Text = eds.fi.Description;

        
            if (eds.di.concreteNodeId!=-1)
                dev.Other.DeviceIdentity.ConcreteNoideId = eds.di.concreteNodeId.ToString();

            dev.Other.DeviceIdentity.VendorName = eds.di.VendorName;
            dev.Other.DeviceIdentity.VendorNumber = eds.di.VendorNumber;

            dev.Other.File = new File();

            dev.Other.File.FileName = eds.fi.FileName;

            dev.Other.File.FileCreationDate = eds.fi.CreationDateTime.ToString("MM-dd-yyyy");
            dev.Other.File.FileCreationTime = eds.fi.CreationDateTime.ToString("h:mmtt");
            dev.Other.File.FileCreator = eds.fi.CreatedBy;

            dev.Other.File.FileModificationDate = eds.fi.ModificationDateTime.ToString("MM-dd-yyyy");
            dev.Other.File.FileModificationTime = eds.fi.ModificationDateTime.ToString("h:mmtt");
            dev.Other.File.FileModifedBy = eds.fi.ModifiedBy;

            dev.Other.File.FileVersion = eds.fi.FileVersion.ToString();
            dev.Other.File.FileRevision = eds.fi.FileRevision;

            dev.Other.File.ExportFolder = eds.fi.exportFolder;

            return dev;
        }


        public Characteristic makecharcteristic(string name,string content)
        {
            Characteristic cl = new Characteristic();

            cl.CharacteristicName = new CharacteristicName();
            cl.CharacteristicContent = new CharacteristicContent();
            cl.CharacteristicContent.Label = new Label();
            cl.CharacteristicName.Label = new Label();

            cl.CharacteristicName.Label.Text = name;
            cl.CharacteristicContent.Label.Text = content;

            return cl;
        }

        public EDSsharp convert(Device dev)
        {
            EDSsharp eds = new EDSsharp();
            
            foreach(CANopenObject coo in dev.CANopenObjectList.CANopenObject)
            {
                ODentry entry = new ODentry();
                entry.index = Convert.ToUInt16(coo.Index, 16);
                entry.parameter_name = coo.Name;
             
                if (coo.AccessType != null)
                {
                    string at = coo.AccessType;

                    Regex reg = new Regex(@"^cons$");
                    at = reg.Replace(at,"const");

                    entry.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), at);
                }

                if (coo.DataType != null)
                {
                    byte datatype = Convert.ToByte(coo.DataType, 16);
                    entry.datatype = (DataType)datatype;
                }
                else
                {
                    //CanOpenNode Project XML did not correctly set DataTypes for record sets

                    if (entry.index == 0x1018)
                        entry.datatype = DataType.IDENTITY;

                    if (entry.index >= 0x1200 && entry.index<0x1400) //check me is this the correct range??
                        entry.datatype = DataType.SDO_PARAMETER;


                    if (entry.index >= 0x1400 && entry.index < 0x1600) //check me is this the correct range??
                        entry.datatype = DataType.PDO_COMMUNICATION_PARAMETER;


                    if (entry.index >= 0x1600 && entry.index < 0x1800) //check me is this the correct range??
                        entry.datatype = DataType.PDO_MAPPING;


                    if (entry.index >= 0x1800 && entry.index < 0x1a00) //check me is this the correct range??
                        entry.datatype = DataType.PDO_COMMUNICATION_PARAMETER;


                    if (entry.index >= 0x1a00 && entry.index < 0x1c00) //check me is this the correct range??
                        entry.datatype = DataType.PDO_MAPPING;


                }

               
                entry.objecttype = (ObjectType)Enum.Parse(typeof(ObjectType),coo.ObjectType);

                entry.defaultvalue = coo.DefaultValue;
                //entry.nosubindexes = Convert.ToInt16(coo.SubNumber);

                if (coo.PDOmapping != null)
                    entry.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), coo.PDOmapping);
                else
                    entry.PDOtype = PDOMappingType.no;

                entry.TPDODetectCos = coo.TPDOdetectCOS == "true";
                entry.AccessFunctionName = coo.AccessFunctionName;
                entry.AccessFunctionPreCode = coo.AccessFunctionPreCode;
                entry.Disabled = coo.Disabled == "true";

                if (coo.Description!=null)
                    entry.Description = coo.Description.Text; //FIXME URL/LANG

                if(coo.Label!=null)
                    entry.Label = coo.Label.Text; //FIXME LANG
                
                if(coo.MemoryType!=null)
                    entry.location = (StorageLocation)Enum.Parse(typeof(StorageLocation), coo.MemoryType);
                
                eds.ods.Add(entry.index, entry);

                if (entry.index == 0x1000 || entry.index==0x1001 || entry.index==0x1018)
                {
                    eds.md.objectlist.Add(eds.md.objectlist.Count+1,entry.index);
                }
                else
                if (entry.index >= 0x2000 && entry.index<0x6000)
                {
                    eds.mo.objectlist.Add(eds.mo.objectlist.Count+1,entry.index);
                }
                else
                {
                     eds.oo.objectlist.Add(eds.oo.objectlist.Count+1,entry.index);
                }

                
                foreach(CANopenSubObject coosub in coo.CANopenSubObject)
                {

                    ODentry subentry = new ODentry();

                    subentry.parameter_name = coosub.Name;
                    subentry.index = entry.index;

                    if(coosub.AccessType!=null)
                        subentry.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), coosub.AccessType);

                    if (coosub.DataType != null)
                    {
                        byte datatype = Convert.ToByte(coosub.DataType, 16);
                        subentry.datatype = (DataType)datatype;
                    }

                    subentry.defaultvalue = coosub.DefaultValue;

                    subentry.subindex = Convert.ToUInt16(coosub.SubIndex, 16);
                    
                    if(coosub.PDOmapping!=null)
                        subentry.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), coosub.PDOmapping);

                    if (entry.objecttype == ObjectType.ARRAY)
                    {
                        subentry.PDOtype = entry.PDOtype;
                    }

                    subentry.location = entry.location;
                    subentry.parent = entry;

                    subentry.objecttype = ObjectType.VAR;

                    if(coosub.TPDOdetectCOS!=null)
                    {
                        subentry.TPDODetectCos = coosub.TPDOdetectCOS == "true";  
                    }
                    else
                    {
                        if(coo.TPDOdetectCOS!=null)
                            subentry.TPDODetectCos = coo.TPDOdetectCOS == "true";
                    }
                       

                    entry.subobjects.Add(subentry.subindex,subentry);

                }
            }

            eds.du.Dummy0001 = dev.Other.DummyUsage.Dummy[0].Entry == "Dummy0001=1";
            eds.du.Dummy0002 = dev.Other.DummyUsage.Dummy[1].Entry == "Dummy0002=1";
            eds.du.Dummy0003 = dev.Other.DummyUsage.Dummy[2].Entry == "Dummy0003=1";
            eds.du.Dummy0004 = dev.Other.DummyUsage.Dummy[3].Entry == "Dummy0004=1";
            eds.du.Dummy0005 = dev.Other.DummyUsage.Dummy[4].Entry == "Dummy0005=1";
            eds.du.Dummy0006 = dev.Other.DummyUsage.Dummy[5].Entry == "Dummy0006=1";
            eds.du.Dummy0007 = dev.Other.DummyUsage.Dummy[6].Entry == "Dummy0007=1";

            foreach(SupportedBaudRate baud in dev.Other.BaudRate.SupportedBaudRate)
            {
                if (baud.Value == "10 Kbps")
                    eds.di.BaudRate_10 = true;
                if (baud.Value == "20 Kbps")
                    eds.di.BaudRate_20 = true;
                if (baud.Value == "50 Kbps")
                    eds.di.BaudRate_50 = true;
                if (baud.Value == "125 Kbps")
                    eds.di.BaudRate_125 = true;
                if (baud.Value == "250 Kbps")
                    eds.di.BaudRate_250 = true;
                if (baud.Value == "500 Kbps")
                    eds.di.BaudRate_500 = true;
                if (baud.Value == "800 Kbps")
                    eds.di.BaudRate_800 = true;
                if (baud.Value == "1000 Kbps")
                    eds.di.BaudRate_1000 = true;

            }

            Dictionary<string, string> keypairs = new Dictionary<string, string>();

            if (dev.Other.Capabilities != null)
            {
                if (dev.Other.Capabilities.CharacteristicsList != null)
                {
                    foreach (Characteristic c in dev.Other.Capabilities.CharacteristicsList.Characteristic)
                    {
                        try
                        {
                            keypairs.Add(c.CharacteristicName.Label.Text, c.CharacteristicContent.Label.Text);
                        }
                        catch (Exception e)
                        {
                            // Warnings.warning_list.Add("Parsing characteristics failed " + e.ToString());
                        }
                    }
                }
            }
           

            bool boolout;
            if (keypairs.ContainsKey("SimpleBootUpSlave") && bool.TryParse(keypairs["SimpleBootUpSlave"], out boolout))
                eds.di.SimpleBootUpSlave = boolout;
            if (keypairs.ContainsKey("SimpleBootUpMaster") && bool.TryParse(keypairs["SimpleBootUpMaster"], out boolout))
                eds.di.SimpleBootUpMaster = boolout;
            if (keypairs.ContainsKey("DynamicChannelsSupported") && bool.TryParse(keypairs["DynamicChannelsSupported"], out boolout))
                eds.di.DynamicChannelsSupported = boolout;
            if (keypairs.ContainsKey("CompactPDO") && bool.TryParse(keypairs["CompactPDO"], out boolout))
                eds.di.CompactPDO = boolout;
            if (keypairs.ContainsKey("GroupMessaging") && bool.TryParse(keypairs["GroupMessaging"], out boolout))
                eds.di.GroupMessaging = boolout;
            if (keypairs.ContainsKey("LSS_Supported") && bool.TryParse(keypairs["LSS_Supported"], out boolout))
                eds.di.LSS_Supported = boolout;

            byte byteout;
            if (keypairs.ContainsKey("Granularity") && byte.TryParse(keypairs["Granularity"], out byteout))
                eds.di.Granularity = byteout;


            eds.di.ProductName = dev.Other.DeviceIdentity.ProductName;
            eds.di.ProductNumber = dev.Other.DeviceIdentity.ProductNumber;

            if(dev.Other.DeviceIdentity.ProductText!=null && dev.Other.DeviceIdentity.ProductText.Description!=null & dev.Other.DeviceIdentity.ProductText.Description.Text!=null)
                eds.fi.Description = dev.Other.DeviceIdentity.ProductText.Description.Text;

            eds.di.VendorName = dev.Other.DeviceIdentity.VendorName;
            eds.di.VendorNumber = dev.Other.DeviceIdentity.VendorNumber;

            if (dev.Other.DeviceIdentity.ConcreteNoideId != null)
            {
                eds.di.concreteNodeId = Convert.ToByte(dev.Other.DeviceIdentity.ConcreteNoideId);
            }
            else
            {
                eds.di.concreteNodeId = -1;
            }

            string dtcombined;

            eds.fi.FileName = dev.Other.File.FileName;

            dtcombined = string.Format("{0} {1}", dev.Other.File.FileCreationTime, dev.Other.File.FileCreationDate);
            try
            {
                eds.fi.CreationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                eds.fi.CreationDate = eds.fi.CreationDateTime.ToString("MM-dd-yyyy");
                eds.fi.CreationTime = eds.fi.CreationDateTime.ToString("h:mmtt");

            }
            catch(Exception e) { }

            eds.fi.CreatedBy = dev.Other.File.FileCreator;
            eds.fi.exportFolder = dev.Other.File.ExportFolder;

            dtcombined = string.Format("{0} {1}", dev.Other.File.FileModificationTime, dev.Other.File.FileModificationDate);
            try
            {
                eds.fi.ModificationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                eds.fi.ModificationDate = eds.fi.ModificationDateTime.ToString("MM-dd-yyyy");
                eds.fi.ModificationTime = eds.fi.ModificationDateTime.ToString("h:mmtt");
            }
            catch (Exception e) { }



  
            eds.fi.ModifiedBy = dev.Other.File.FileModifedBy;


            dev.Other.Capabilities = dev.Other.Capabilities;

            try
            {
                eds.fi.FileVersion = Convert.ToByte(dev.Other.File.FileVersion);
            }
            catch (Exception e)
            {
                if (dev.Other.File != null)
                {
                    // CanOpenNode default project.xml conatins - for fileversion, its suspose to be a byte field according to DS306
                    if (dev.Other.File.FileVersion == "-")
                    {
                        dev.Other.File.FileVersion = "0";
                    }
                    else
                    {
                        Warnings.warning_list.Add(String.Format("Unable to parse FileVersion\"{0}\" {1}", dev.Other.File.FileVersion, e.ToString()));
                    }
                }

                eds.fi.FileVersion = 0;
            }

            eds.fi.FileRevision = dev.Other.File.FileRevision;

            eds.fi.EDSVersion = "4.0";
            
            //FIX me any other approprate defaults for eds here??

            eds.updatePDOcount();

            return eds;
        }

    }
}
