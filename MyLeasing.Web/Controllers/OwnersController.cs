﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;
using MyLeasing.Web.Models;

namespace MyLeasing.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class OwnersController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IConverterHerper _ConverterHerper;
        private readonly IImageHelper _imageHelper;

        public OwnersController(
            DataContext dataContext,
            IUserHelper userHelper,
            ICombosHelper combosHelper,
            IConverterHerper ConverterHerper,
            IImageHelper imageHelper)

        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _ConverterHerper = ConverterHerper;
            _imageHelper = imageHelper;
        }

        public IActionResult Index()
        {
            //inner join
            return View(_dataContext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .Include(o => o.contracts));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _dataContext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .ThenInclude(p => p.propertyImages)
                .Include(o => o.contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

       public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await CreateUserAsync(model);
                if (user != null)
                {
                    var owner = new Owner
                    {
                        contracts = new List<Contract>(),
                        Properties = new List<Property>(),
                        User = user
                    };

                    _dataContext.Owners.Add(owner);
                    await _dataContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "User with this email already exist.");
            }

            return View(model);
        }

        //  metodo crear usuario
        private async Task<User> CreateUserAsync(AddUserViewModel model)
        {
            var user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Username

            };

            var result = await _userHelper.AddUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                user = await _userHelper.GetUserByEmailAsync(model.Username);
                await _userHelper.AddUserToRoleAsync(user, "Owner");
                return user;
            }
            return null;
        }


        private async Task<User> AddUser(AddUserViewModel view)
        {
            var user = new User
            {
                Address = view.Address,
                Document = view.Document,
                Email = view.Username,
                FirstName = view.FirstName,
                LastName = view.LastName,
                PhoneNumber = view.PhoneNumber,
                UserName = view.Username
            };

            var result = await _userHelper.AddUserAsync(user, view.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            var newUser = await _userHelper.GetUserByEmailAsync(view.Username);
            await _userHelper.AddUserToRoleAsync(newUser, "Owner");
            return newUser;
        }

       //combo 
        public async Task<IActionResult> AddProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _dataContext.Owners.FindAsync(id.Value);
            if (owner == null)
            {
                return NotFound();
            }

            var model = new PropertyViewModel
            {
                OwnerId = owner.Id,
                PropertyTypes = _combosHelper.GetComboPropertyTypes()
            };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var property = await _ConverterHerper.ToPropertyAsync(model, true);
                _dataContext.Properties.Add(property);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction($"Details/{model.OwnerId}");
            }

            return View(model);
        }

        public async Task<IActionResult> EditProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _dataContext.Properties
                .Include(p => p.Owner)
                .Include(p => p.PropertyType)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (property == null)
            {
                return NotFound();
            }

            var model = _ConverterHerper.ToPropertyViewModel(property);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProperty(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var property = await _ConverterHerper.ToPropertyAsync(model, false);
                _dataContext.Properties.Update(property);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction($"Details/{model.OwnerId}");
            }

            return View(model);
        }

        //Get
        public async Task<IActionResult> DetailsProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //vas a buscar  en las propiedades cuyo codigo es por el id
            //e incluya el owner,user,contracts etc...
            var property = await _dataContext.Properties
                .Include(o => o.Owner)
                .ThenInclude(o => o.User)
                .Include(o => o.Contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .Include(o => o.PropertyType)
                .Include(p => p.propertyImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (property == null)
            {
                //propiedad no existe 
                return NotFound();
            }
            //devuelva el objeto property
            return View(property);
        }

        //id de la propiedad
        public async Task<IActionResult> AddImage(int? id)
        {
            //id que no sea igual a null
            if (id == null)
            {
                return NotFound();
            }

            //se busca en la base de datos en la collecion propiedad por el id
            var property = await _dataContext.Properties.FindAsync(id.Value);
            if (property == null)
            {
                return NotFound();
            }
            // el modelo va hacer un nuevo propertyimageviewmodel
            var model = new PropertyImageViewModel
            {
                //se manda al modelo el id
                Id = property.Id
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(PropertyImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = string.Empty;

                if (model.ImageFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile);
                }

                var propertyImage = new PropertyImage
                {
                    ImageUrl = path,
                    Property = await _dataContext.Properties.FindAsync(model.Id)
                };
                //agragamos la propiedad
                _dataContext.propertyImages.Add(propertyImage);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction($"{nameof(DetailsProperty)}/{model.Id}");
            }

            return View(model);
        }

        //se le pasa el id de la propiedad
        public async Task<IActionResult> AddContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _dataContext.Properties
                //se incluye el owner para pasar al modelo
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (property == null)
            {
                return NotFound();
            }
            //enviar a la propiedad
            var model = new ContractViewModel
            {
                OwnerId = property.Owner.Id,
                PropertyId = property.Id,
                //se llama al metodo GetComboLessees
                Lessees = _combosHelper.GetComboLessees(),
                Price = property.Price,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddContract(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contract = await _ConverterHerper.ToContractAsync(model, true);
                _dataContext.contracts.Add(contract);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction($"{nameof(DetailsProperty)}/{model.PropertyId}");
            }

            return View(model);
        }

        //editar contrato 
        //id de contrato
        public async Task<IActionResult> EditContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _dataContext.contracts
                .Include(p => p.Owner)
                .Include(p => p.Lessee)
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            return View(_ConverterHerper.ToContractViewModel(contract));
        }

        [HttpPost]
        public async Task<IActionResult> EditContract(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contract = await _ConverterHerper.ToContractAsync(model, false);
                _dataContext.contracts.Update(contract);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction($"{nameof(DetailsProperty)}/{model.PropertyId}");
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _dataContext.Owners
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id.Value);
            if (owner == null)
            {
                return NotFound();
            }
            //cargamos a model con lod datos de owner.user
            var model = new EditUserViewModel
            {
                Address = owner.User.Address,
                Document = owner.User.Document,
                FirstName = owner.User.FirstName,
                Id = owner.Id,
                LastName = owner.User.LastName,
                PhoneNumber = owner.User.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var owner = await _dataContext.Owners
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == model.Id);

                owner.User.Document = model.Document;
                owner.User.FirstName = model.FirstName;
                owner.User.LastName = model.LastName;
                owner.User.Address = model.Address;
                owner.User.PhoneNumber = model.PhoneNumber;

                await _userHelper.UpdateUserAsync(owner.User);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }



        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propertyImage = await _dataContext.propertyImages
                .Include(pi => pi.Property)
                .FirstOrDefaultAsync(pi => pi.Id == id.Value);
            if (propertyImage == null)
            {
                return NotFound();
            }

            _dataContext.propertyImages.Remove(propertyImage);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction($"{nameof(DetailsProperty)}/{propertyImage.Property.Id}");
        }

        public async Task<IActionResult> DeleteContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _dataContext.contracts
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            _dataContext.contracts.Remove(contract);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction($"{nameof(DetailsProperty)}/{contract.Property.Id}");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _dataContext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            if(owner.Properties.Count != 0)
            {
                ModelState.AddModelError(string.Empty, "Owner can`t be delete because it has properties");
                return RedirectToAction(nameof(Index));
            }


            _dataContext.Owners.Remove(owner);
            await _dataContext.SaveChangesAsync();
            await _userHelper.DeleteUserAsync(owner.User.Email);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _dataContext.Properties
                .Include(p => p.Owner)
                .Include(p => p.propertyImages)
                .Include(p => p.Contracts)
                .FirstOrDefaultAsync(pi => pi.Id == id.Value);
            if (property == null)
            {
                return NotFound();
            }

            if(property.Contracts.Count != 0)
            {
                ModelState.AddModelError(string.Empty, "The property can`t be deleted because it has contracs.");
                return RedirectToAction($"{nameof(Details)}/{property.Owner.Id}");
            }

            _dataContext.propertyImages.RemoveRange(property.propertyImages);
            _dataContext.Properties.Remove(property);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction($"{nameof(Details)}/{property.Owner.Id}");
        }


    }
}
