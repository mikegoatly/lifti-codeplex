﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lifti.Properties {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Lifti.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Token expected: {0}.
        /// </summary>
        internal static string ExpectedToken {
            get {
                return ResourceManager.GetString("ExpectedToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to serialize the index - the number of index entries serialized to the file differs from the number stored in the index. This might indicate a corrupted index..
        /// </summary>
        internal static string IndexCountDiffersFromLookupCount {
            get {
                return ResourceManager.GetString("IndexCountDiffersFromLookupCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to deserialize from the given stream as the version of the serialized file is not compatible with this version of LIFTI..
        /// </summary>
        internal static string InvalidFileVersion {
            get {
                return ResourceManager.GetString("InvalidFileVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to deserialize - the serialized type {0} differs from the type in the provided index {1}..
        /// </summary>
        internal static string SerializedTypeDiffers {
            get {
                return ResourceManager.GetString("SerializedTypeDiffers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided stream does not contain the expected header bytes and is either corrupt or not a serialized index..
        /// </summary>
        internal static string StreamDoesNotContainExpectedHeaderMarker {
            get {
                return ResourceManager.GetString("StreamDoesNotContainExpectedHeaderMarker", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to automatically deserialize an index containing type {0}. Please use Deserialize overload to provide the key deserialization method..
        /// </summary>
        internal static string UnableToAutomaticallyDeserializeType {
            get {
                return ResourceManager.GetString("UnableToAutomaticallyDeserializeType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to automatically serialize an index containing type {0}. Please use Serialize overload to provide the key serialization method..
        /// </summary>
        internal static string UnableToAutomaticallySerializeType {
            get {
                return ResourceManager.GetString("UnableToAutomaticallySerializeType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to determine the full name of the index key type..
        /// </summary>
        internal static string UnableToDetermineFullNameOfKeyType {
            get {
                return ResourceManager.GetString("UnableToDetermineFullNameOfKeyType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal error - Unable to perform positional match between item matches representing different items..
        /// </summary>
        internal static string UnableToPerformPositionalMatchBetweenDifferentItems {
            get {
                return ResourceManager.GetString("UnableToPerformPositionalMatchBetweenDifferentItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The query ended unexpectedly - a token was expected..
        /// </summary>
        internal static string UnexpectedEndOfQuery {
            get {
                return ResourceManager.GetString("UnexpectedEndOfQuery", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unexpected operator was encountered: {0}.
        /// </summary>
        internal static string UnexpectedOperator {
            get {
                return ResourceManager.GetString("UnexpectedOperator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal error - An unexpected operator was encountered: {0}.
        /// </summary>
        internal static string UnexpectedOperatorInternal {
            get {
                return ResourceManager.GetString("UnexpectedOperatorInternal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected token encountered: {0}.
        /// </summary>
        internal static string UnexpectedTokenEncountered {
            get {
                return ResourceManager.GetString("UnexpectedTokenEncountered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown query operator: {0}.
        /// </summary>
        internal static string UnknownOperator {
            get {
                return ResourceManager.GetString("UnknownOperator", resourceCulture);
            }
        }
    }
}
