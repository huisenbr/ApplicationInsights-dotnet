﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class DictionarySerializationWriter : ISerializationWriter
    {   
        internal const string DefaultKey = "Key";
        internal const string DefaultObjectKey = "Obj";

        private readonly Stack<string> lastPrefix = new Stack<string>(); // Stores previous prefix for the property Key
        private readonly Stack<long> lastIndex = new Stack<long>(); // Stores previous index for unnamed property
        private string currentPrefix = string.Empty; // Prefix is prepended before property name to distinguish depth and sequences in the flat list
        private long currentIndex = 1; // Index is appended to "Key" to provide a property name for objects with no names

        internal DictionarySerializationWriter()
        {
            this.AccumulatedDictionary = new Dictionary<string, string>();
            this.AccumulatedMeasurements = new Dictionary<string, double>();
        }

        internal Dictionary<string, string> AccumulatedDictionary { get; }
        
        internal Dictionary<string, double> AccumulatedMeasurements { get; }

        public void WriteProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                this.AccumulatedDictionary[key] = value;
            }
        }

        public void WriteProperty(string name, double? value) // Should this be measurement? What if it's empty? What if it's indeed property?
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                if (value.HasValue)
                {
                    this.AccumulatedDictionary[key] = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    this.AccumulatedDictionary[key] = null;
                }
            }
        }

        public void WriteProperty(string name, int? value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                if (value.HasValue)
                {
                    this.AccumulatedDictionary[key] = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    this.AccumulatedDictionary[key] = null;
                }
            }
        }

        public void WriteProperty(string name, bool? value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                if (value.HasValue)
                {
#if NETSTANDARD
                    this.AccumulatedDictionary[key] = value.Value.ToString();
#else
                    this.AccumulatedDictionary[key] = value.Value.ToString(CultureInfo.InvariantCulture);
#endif
                }
                else
                {
                    this.AccumulatedDictionary[key] = null;
                }
            }
        }

        public void WriteProperty(string name, TimeSpan? value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                if (value.HasValue)
                {
                    this.AccumulatedDictionary[key] = value.Value.ToString();
                }
                else
                {
                    this.AccumulatedDictionary[key] = null;
                }
            }
        }

        public void WriteProperty(string name, DateTimeOffset? value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }
            
            string key = this.GetKey(name);

            if (!this.AccumulatedDictionary.ContainsKey(key))
            {
                if (value.HasValue)
                {
                    this.AccumulatedDictionary[key] = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    this.AccumulatedDictionary[key] = null;
                }
            }
        }

        public void WriteProperty(string name, ISerializableWithWriter value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            if (value != null)
            {
                this.lastPrefix.Push(this.currentPrefix);
                this.currentPrefix = this.GetKey(name);
                value.Serialize(this);
                this.currentPrefix = this.lastPrefix.Pop();
            }
            else
            {
                this.WriteProperty(this.GetKey(name), (string)null);
            }
        }

        public void WriteProperty(ISerializableWithWriter value)
        {
            if (value != null)
            {
                this.lastPrefix.Push(this.currentPrefix);
                this.currentPrefix = this.GenerateSequencialPrefix();
                value.Serialize(this);
                this.currentPrefix = this.lastPrefix.Pop();
            } // Generated name would not indicate which object is missing, hence useless, no entry in accumulated dictionary 
        }

        public void WriteProperty(string name, IList<string> items)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    string key = this.GetKey(name + i);
                    if (!this.AccumulatedDictionary.ContainsKey(key))
                    {
                        this.AccumulatedDictionary[key] = items[i];
                    }
                }
            }
            else
            {
                this.WriteProperty(this.GetKey(name), (string)null);
            }
        }

        public void WriteProperty(string name, IList<ISerializableWithWriter> items)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] != null)
                    {
                        this.lastPrefix.Push(this.currentPrefix);
                        this.currentPrefix = this.GetKey(name + i);
                        items[i].Serialize(this);
                        this.currentPrefix = this.lastPrefix.Pop();
                    }
                    else
                    {
                        this.AccumulatedDictionary[this.GetKey(name + i)] = null;
                    }
                }
            }
            else
            {
                this.WriteProperty(this.GetKey(name), (string)null);
            }
        }

        public void WriteProperty(string name, IDictionary<string, string> items)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            if (items != null)
            {
                foreach (KeyValuePair<string, string> pair in items)
                {
                    string key = this.GetKey(string.Concat(name, ".", pair.Key));
                    if (!this.AccumulatedDictionary.ContainsKey(key))
                    {
                        this.AccumulatedDictionary[key] = pair.Value;
                    }
                }
            }
            else
            {
                this.WriteProperty(this.GetKey(name), (string)null);
            }
        }

        public void WriteProperty(string name, IDictionary<string, double> items)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            if (items != null)
            {
                foreach (KeyValuePair<string, double> pair in items)
                {
                    string key = this.GetKey(string.Concat(name, ".", pair.Key));
                    if (!this.AccumulatedMeasurements.ContainsKey(key))
                    {
                        this.AccumulatedMeasurements[key] = pair.Value;
                    }
                }
            } // We'd typically like to store Property name with no value, however storing metric name with no value has less benefits
        }

        public void WriteStartObject(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("This argument requires a non-empty value", nameof(name));
            }

            this.lastPrefix.Push(this.currentPrefix);
            this.currentPrefix = this.GetKey(name);
            this.lastIndex.Push(this.currentIndex);
            this.currentIndex = 1;
        }

        public void WriteStartObject()
        {
            this.lastPrefix.Push(this.currentPrefix);
            this.currentPrefix = this.GenerateSequencialPrefixForObject();
            this.lastIndex.Push(this.currentIndex);
            this.currentIndex = 1;
        }

        public void WriteEndObject()
        {
            this.currentPrefix = this.lastPrefix.Pop();
            this.currentIndex = this.lastIndex.Pop();
        }

        private string GetKey(string fieldName)
        {
            return string.IsNullOrEmpty(this.currentPrefix) ? fieldName : string.Concat(this.currentPrefix, ".", fieldName);
        }

        private string GenerateSequencialPrefix()
        {
            return this.GetKey(DefaultKey + this.currentIndex++);
        }

        private string GenerateSequencialPrefixForObject()
        {
            return this.GetKey(DefaultObjectKey + this.currentIndex++);
        }
    }
}