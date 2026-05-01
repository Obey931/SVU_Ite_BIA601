using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace BIA601.Services
{
    public class EvolutionEngine
    {
        private readonly string _conn;
        private List<User> users;
        private List<Product> products;
        private List<Rating> ratings;

        private Random rand = new Random();

        public EvolutionEngine(string conn)
        {
            _conn = conn;

            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR LOADING DATA: " + ex.Message);
                throw;
            }
        }

        void LoadData()
        {
            users = new List<User>();
            products = new List<Product>();
            ratings = new List<Rating>();

            using (var conn = new NpgsqlConnection(_conn))
            {
                conn.Open();
                using (var cmd1 = new NpgsqlCommand("SELECT userid FROM users", conn))
                using (var r1 = cmd1.ExecuteReader())
                {
                    while (r1.Read())
                        users.Add(new User { Id = r1.GetInt32(0) });
                }

                using (var cmd2 = new NpgsqlCommand("SELECT productid FROM products", conn))
                using (var r2 = cmd2.ExecuteReader())
                {
                    while (r2.Read())
                        products.Add(new Product { Id = r2.GetInt32(0) });
                }

                using (var cmd3 = new NpgsqlCommand("SELECT userid, productid, rating FROM ratings", conn))
                using (var r3 = cmd3.ExecuteReader())
                {
                    while (r3.Read())
                    {
                        ratings.Add(new Rating
                        {
                            UserId = r3.GetInt32(0),
                            ProductId = r3.GetInt32(1),
                            Value = r3.GetDouble(2)
                        });
                    }
                }
            }
        }

        public List<int> GetRecommendations(int userId)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return new List<int>();

            var population = InitializePopulation(user);

            for (int gen = 0; gen < 50; gen++)
            {
                population = Evolve(population, user);
            }

            return population
                .OrderByDescending(c => c.Score)
                .First()
                .Genes;
        }

        List<Chromosome> InitializePopulation(User user)
        {
            var pop = new List<Chromosome>();

            for (int i = 0; i < 30; i++)
            {
                var genes = products
                    .OrderBy(x => rand.Next())
                    .Take(5)
                    .Select(p => p.Id)
                    .ToList();

                pop.Add(new Chromosome(genes, Fitness(user, genes)));
            }

            return pop;
        }

        List<Chromosome> Evolve(List<Chromosome> pop, User user)
        {
            var next = new List<Chromosome>();

            while (next.Count < pop.Count)
            {
                var p1 = Tournament(pop);
                var p2 = Tournament(pop);

                var childGenes = Cross(p1.Genes, p2.Genes);
                Mutate(childGenes);

                var score = Fitness(user, childGenes);
                next.Add(new Chromosome(childGenes, score));
            }

            return next;
        }

        Chromosome Tournament(List<Chromosome> pop)
        {
            return pop
                .OrderByDescending(x => x.Score)
                .Take(5)
                .OrderBy(x => rand.Next())
                .First();
        }

        List<int> Cross(List<int> a, List<int> b)
        {
            int pivot = rand.Next(a.Count);

            var result = a.Take(pivot)
                          .Concat(b.Skip(pivot))
                          .Distinct()
                          .Take(5)
                          .ToList();

            while (result.Count < 5 && products.Count > 0)
            {
                var extra = products[rand.Next(products.Count)].Id;
                if (!result.Contains(extra))
                    result.Add(extra);
            }

            return result;
        }

        void Mutate(List<int> genes)
        {
            if (products.Count == 0) return;

            if (rand.NextDouble() < 0.2)
            {
                int index = rand.Next(genes.Count);
                genes[index] = products[rand.Next(products.Count)].Id;
            }
        }

        double Fitness(User user, List<int> genes)
        {
            double score = 0;

            foreach (var g in genes)
            {
                var r = ratings.FirstOrDefault(x =>
                    x.UserId == user.Id &&
                    x.ProductId == g);

                if (r != null)
                    score += r.Value * 2;

                score += rand.NextDouble();
            }

            return score;
        }
    }

    public class Chromosome
    {
        public List<int> Genes { get; }
        public double Score { get; }

        public Chromosome(List<int> genes, double score)
        {
            Genes = genes;
            Score = score;
        }
    }

    public class User
    {
        public int Id { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
    }

    public class Rating
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public double Value { get; set; }
    }
}
