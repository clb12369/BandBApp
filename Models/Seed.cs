using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public static class Seed
{
    public static void Initialize(DB db, bool canCreate, bool mustMigrate)
    {
        if(canCreate) {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        if(mustMigrate) db.Database.Migrate();
        
        if(db.Cards.Any() || db.CardLists.Any()) return;

        Board b = new Board { Title = "Test Board", Lists = new List<CardList>() };

        Action createList = () => {
            CardList todo = new CardList { Summary="Todo items", Cards = new List<Card>() };

            for(var i = 0; i < 10; i++)
                todo.Cards.Add(new Card { Title = $"Test Card {i}", Text = $"Test Content {i}",  });
            
            b.Lists.Add(todo);
        };

        for(var j = 0; j<3; j++)
            createList();
        
        db.Boards.Add(b);
        db.SaveChanges(); 
        Console.WriteLine("----------DB SEEDED-------------");
    }
}