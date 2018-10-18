using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Activity0_Linq : MonoBehaviour
{

    struct Person
    {
        public int age;
        public string country;
        public string name;
    }

    List<Person> persons = new List<Person>
    {
        new Person { age = 10, country="AU", name="sam" },
        new Person { age = 13, country="DE", name="joe" },
        new Person { age = 13, country="DE", name="sally" },
        new Person { age = 33, country="PL", name="jane" },
        new Person { age = 26, country="RU", name="fred" },
        new Person { age = 22, country="RU", name="joey" },
    };


    void Start()
    {

        persons.Where(x => x.country == "DE")
               .Print(x => x.name);

    }

    void Update()
    {

    }

    void PrintList(List<Person> l)
    {
        foreach (var p in l)
        {
            print(p.name);
        }
    }
}

public static class LINQExtension
{
    public static IEnumerable<T> Print<T>(this IEnumerable<T> source, Func<T,string> doit)
    {
        foreach (var p in source)
        {
            Debug.Log(doit.Invoke(p));
        }
        return source;
    }
}