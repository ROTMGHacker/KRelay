﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AntiLag {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class AntiLagConfig : ApplicationSettingsBase {
        
        private static AntiLagConfig defaultInstance = ((AntiLagConfig)(Synchronized(new AntiLagConfig())));
        
        public static AntiLagConfig Default {
            get {
                return defaultInstance;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool Ally {
            get {
                return ((bool)(this["Ally"]));
            }
            set {
                this["Ally"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool Effects {
            get {
                return ((bool)(this["Effects"]));
            }
            set {
                this["Effects"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool Damage {
            get {
                return ((bool)(this["Damage"]));
            }
            set {
                this["Damage"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("True")]
        public bool Other {
            get {
                return ((bool)(this["Other"]));
            }
            set {
                this["Other"] = value;
            }
        }
    }
}
