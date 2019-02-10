using qshine.Audit;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Find difference properties and values compare two objects
    /// </summary>
    public class JsonDiffer
    {
        string _jsonLeft = null;
        string _jsonRight = null;
        /// <summary>
        /// Compare two entity objects
        /// </summary>
        /// <param name="objectLeft">object 1</param>
        /// <param name="objectRight">object 2</param>
        public JsonDiffer(object objectLeft, object objectRight)
        {
            if (objectLeft != null) {
                _jsonLeft = objectLeft.Serialize();
            }

            if (objectRight != null)
            {
                _jsonRight = objectRight.Serialize();
            }
        }

        /// <summary>
        /// Compare two JSON objects
        /// </summary>
        /// <param name="jsonLeft">JSON formatted object 1</param>
        /// <param name="jsonRight">JSON formatted object 2</param>
        public JsonDiffer(string jsonLeft, string jsonRight)
        {
            if (!string.IsNullOrEmpty(jsonLeft))
            {
                _jsonLeft = jsonLeft;
            }

            if (!string.IsNullOrEmpty(jsonRight))
            {
                _jsonRight = jsonRight;
            }
        }

        /// <summary>
        /// Compare two given objects and find different property names and values.
        /// The output JSON objects contain difference of the properties and values.
        /// The same property and value will be ignored.
        /// </summary>
        /// <returns>a list of value difference</returns>
        public Dictionary<string, AuditValue> GetDiff()
        {
            Dictionary<string, object> leftObject;
            Dictionary<string, object> rightObject;
            if (_jsonLeft == null)
            {
                leftObject = new Dictionary<string, object>();
            }
            else
            {
                leftObject = _jsonLeft.DeserializeDictionary();
            }
            if (_jsonRight == null)
            {
                rightObject = new Dictionary<string, object>();
            }
            else
            {
                rightObject = _jsonRight.DeserializeDictionary();
            }
            var dlt = new Dictionary<string, AuditValue>();
            //find all values exists in left object
            foreach(var p in leftObject)
            {
                if (rightObject.ContainsKey(p.Key))
                {
                    if (!IsSame(p.Value, rightObject[p.Key]))
                    {
                        var diffValue = new AuditValue
                        {
                            OldValue = Distinct(p.Value, rightObject[p.Key]),
                            NewValue = Distinct(rightObject[p.Key], p.Value)
                        };

                        dlt.Add(p.Key, diffValue);
                    }
                }
                else
                {
                    dlt.Add(p.Key, new AuditValue
                    {
                        OldValue = p.Value,
                        NewValue = null
                    });
                }
            }
            //find all values exists in right object only
            foreach (var p in rightObject)
            {
                if (!leftObject.ContainsKey(p.Key))
                {
                    dlt.Add(p.Key, new AuditValue
                    {
                        OldValue = null,
                        NewValue = p.Value
                    });
                }
            }

            return dlt;
        }

        bool IsSame(object p1, object p2)
        {
            //check basic
            if (p1 == null) {
                return null == p2;
            }
            if (p2 == null) {
                return false;
            }

            if (object.ReferenceEquals(p1, p2)) {
                return true;
            }

            var dictionaryObject1 = p1 as Dictionary<string, object>;
            var dictionaryObject2 = p2 as Dictionary<string, object>;

            if(dictionaryObject1 != null && dictionaryObject1!=null)
            {
                // check keys are the same
                foreach (var k in dictionaryObject1.Keys) {
                    if (!dictionaryObject2.ContainsKey(k))
                    {
                        return false;
                    }
                }

                // check values are the same
                foreach (var k in dictionaryObject1.Keys)
                {
                    if (!IsSame(dictionaryObject1[k], dictionaryObject2[k]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                var listObject1 = p1 as IEnumerable<object>;
                var listObject2 = p2 as IEnumerable<object>;

                if (listObject1 != null && listObject2 != null)
                {
                    return listObject1.Except(listObject2).Count() == 0;
                }
                else
                {
                    return p1.Equals(p2);
                }
            }

        }

        object Distinct(object left, object right)
        {
            var dictionaryObject1 = left as Dictionary<string, object>;
            var dictionaryObject2 = right as Dictionary<string, object>;

            if (dictionaryObject1!=null && dictionaryObject2!=null)
            {
                //return dictionaryObject1.Except(dictionaryObject2).ToDictionary(x => x.Key, x => x.Value);

                var dlt = new Dictionary<string, object>();
                //find all diff values exists in left object
                foreach (var p in dictionaryObject1)
                {
                    if (dictionaryObject2.ContainsKey(p.Key))
                    {
                        if (!IsSame(p.Value, dictionaryObject2[p.Key]))
                        {
                            dlt.Add(p.Key, Distinct(p.Value, dictionaryObject2[p.Key]));
                        }
                    }
                    else
                    {
                        dlt.Add(p.Key, p.Value);
                    }
                }
                return dlt;
            }
            else
            {
                var listObject1 = left as IEnumerable<object>;
                var listObject2 = right as IEnumerable<object>;

                if (listObject1 != null && listObject2 != null)
                {
                    return listObject1.Except(listObject2).ToList<object>();
                }
                else
                {
                    return left;
                }
            }
        }
    }
}
