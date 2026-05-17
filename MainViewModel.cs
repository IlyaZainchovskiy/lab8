using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace lab8
{
    public class MainViewModel : ViewModelBase
    {
        private readonly CookbookDbContext _db;

        public ObservableCollection<Recipe> Recipes { get; set; }

        private string _newRecipeTitle;
        public string NewRecipeTitle
        {
            get => _newRecipeTitle;
            set { _newRecipeTitle = value; OnPropertyChanged(); AddCommand.RaiseCanExecuteChanged(); }
        }

        private string _newRecipeInstructions;
        public string NewRecipeInstructions
        {
            get => _newRecipeInstructions;
            set { _newRecipeInstructions = value; OnPropertyChanged(); }
        }

        public int TotalCount => Recipes?.Count ?? 0;

        public RelayCommand AddCommand { get; }
        public RelayCommand SortCommand { get; }

        public MainViewModel()
        {
            _db = new CookbookDbContext();
            _db.Database.EnsureCreated();

            if (!_db.Categories.Any())
            {
                _db.Categories.Add(new Category { Name = "Основні страви" });
                _db.SaveChanges();
            }

            Recipes = new ObservableCollection<Recipe>();
            LoadData();

            AddCommand = new RelayCommand(AddRecipe, CanAddRecipe);
            SortCommand = new RelayCommand(SortRecipes);
        }

        private void LoadData()
        {
            var dbRecipes = _db.Recipes.Include(r => r.Category).ToList();
            Recipes.Clear();
            foreach (var r in dbRecipes)
            {
                Recipes.Add(r);
            }
            OnPropertyChanged(nameof(TotalCount)); 
        }

        private bool CanAddRecipe()
        {
            return !string.IsNullOrWhiteSpace(NewRecipeTitle);
        }

        private void AddRecipe()
        {
            var category = _db.Categories.FirstOrDefault();
            var newRecipe = new Recipe
            {
                Title = NewRecipeTitle,
                Instructions = NewRecipeInstructions,
                CategoryId = category?.CategoryId ?? 1
            };

            _db.Recipes.Add(newRecipe);
            _db.SaveChanges();

            Recipes.Add(newRecipe);
            OnPropertyChanged(nameof(TotalCount)); 

            NewRecipeTitle = string.Empty;
            NewRecipeInstructions = string.Empty;
        }

        private void SortRecipes()
        {
            var sorted = Recipes.OrderBy(r => r.Title).ToList();
            Recipes.Clear();
            foreach (var r in sorted)
            {
                Recipes.Add(r);
            }
        }
    }
}
