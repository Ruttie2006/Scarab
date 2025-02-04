﻿using System.Collections.Generic;
using System.Linq;
using Scarab.Models;

namespace Scarab.Services;

public class ReverseDependencySearch
{
    // a dictionary to allow constant lookup times of ModItems from name
    private readonly Dictionary<string, ModItem> _items;

    public ReverseDependencySearch(IEnumerable<ModItem> allModItems)
    {
        // no need to add non modlinks mod because they dont have a dependency tree
        _items = allModItems.Where(x => x.State is not NotInModLinksState)
            .ToDictionary(x => x.Name, x => x);
    }

    public IEnumerable<ModItem> GetAllDependentAndIntegratedMods(ModItem item)
    {
        var dependants = new List<ModItem>();
        
        foreach (var mod in _items.Values)
        {
            if (mod.HasIntegrations)
            {
                if (mod.Integrations.Contains(item.Name))
                {
                    dependants.Add(mod);
                }
            }
            
            if (IsDependent(mod, item))
            {
                dependants.Add(mod);
            }
        }
        return dependants;
    }
    
    public IEnumerable<ModItem> GetAllEnabledDependents(ModItem item)
    {
        var dependants = new List<ModItem>();
        
        // check all enabled mods if they have a dependency on this mod
        foreach (var mod in _items.Values.Where(x => x.EnabledIsChecked))
        {
            if (IsDependent(mod, item))
            {
                dependants.Add(mod);
            }
        }
        return dependants;
    }

    private bool IsDependent(ModItem mod, ModItem targetMod)
    {
        foreach (var dependency in mod.Dependencies.Select(x => _items[x]))
        {
            // if the mod's listed dependency is the targetMod, it is a dependency
            if (dependency == targetMod) 
                return true;

            // it also is a dependent if it has a transitive dependent
            if (IsDependent(dependency, targetMod)) 
                return true;
        }

        return false;
    }
}