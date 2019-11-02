using FluentNHibernate.Mapping;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHibernate_timing.NHibernateMapping
{
    public class User
    {
        public virtual int Id { get; set; } 

        public virtual string Name { get; set; }

        public virtual string LastName { get; set; }
    }

    public class UserMapping: ClassMap<User>
    {
        public UserMapping()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.LastName);
        }
    }
}
