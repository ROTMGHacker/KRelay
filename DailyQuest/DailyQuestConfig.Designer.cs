﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DailyQuest {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class DailyQuestConfig : global::System.Configuration.ApplicationSettingsBase {
        
        private static DailyQuestConfig defaultInstance = ((DailyQuestConfig)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DailyQuestConfig())));
        
        public static DailyQuestConfig Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoRequest {
            get {
                return ((bool)(this["AutoRequest"]));
            }
            set {
                this["AutoRequest"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool BagNotifications {
            get {
                return ((bool)(this["BagNotifications"]));
            }
            set {
                this["BagNotifications"] = value;
            }
        }
    }
}
