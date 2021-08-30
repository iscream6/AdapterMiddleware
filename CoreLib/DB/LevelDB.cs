using LevelDB;

using System;
using System.Collections.Generic;
using System.IO;

namespace NpmDataBase
{
    public class NpmDB
    {
        public DB OpenDB()
        {
            try
            {
                Options opt = new Options { CreateIfMissing = true };
                return new DB(opt, $"{Directory.GetCurrentDirectory()}\\LevelDB");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Close(DB db)
        {
            db.Close();
        }

        public string Read(DB db, string key)
        {
            return db.Get(key);
        }

        public void Write(DB db, string key, string value)
        {
            db.Put(key, value);
        }

        public List<string> GetKeys(DB db)
        {
            List<string> keys = new List<string>();
            using (var iterator = db.CreateIterator())
            {
                //Iterate to print the keys as strings.
                for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
                {
                    keys.Add(iterator.KeyAsString());
                }
            }

            return keys;
        }

        public SnapShot GetSnapShot(DB db)
        {
            SnapShot snapShot = db.CreateSnapshot();
            return snapShot;
        }
    }
}
