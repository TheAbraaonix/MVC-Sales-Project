using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SalesWebMVC.Models;
using SalesWebMVC.Services;
using SalesWebMVC.Services.Exceptions;

namespace SalesWebMVC.Controllers
{
	public class SellersController : Controller
	{
		private readonly SellerService _sellerService;
		private readonly DepartmentService _departmentService;

		public SellersController(SellerService sellerService, DepartmentService departmentService)
		{
			_sellerService = sellerService;
			_departmentService = departmentService;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _sellerService.FindAllAsync();
			return View(list);
		}

		public async Task<IActionResult> Create()
		{
			var departments = await _departmentService.FindAllAsync();
			var viewModel = new SellerFormViewModel { Departments = departments };
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Seller obj)
		{
			if (!ModelState.IsValid) 
			{ 
				var departments = await _departmentService.FindAllAsync();
				var viewModel = new SellerFormViewModel {Seller = obj, Departments = departments };
				return View(viewModel);  
			}

			await _sellerService.InsertAsync(obj);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return RedirectToAction(nameof(Error), new { Message = "Id not provided" });

			var obj = await _sellerService.FindByIdAsync(id.Value);

			if (obj == null) return RedirectToAction(nameof(Error), new { Message = "Id not found" });

			return View(obj);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _sellerService.RemoveAsync(id);
				return RedirectToAction(nameof(Index));
			}
			catch (IntegrityException)
			{
				return RedirectToAction(nameof(Error), new { Message = "Can not delete seller because it has sales active" });
			}
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return RedirectToAction(nameof(Error), new { Message = "Id not provided" });

			var obj = await _sellerService.FindByIdAsync(id.Value);

			if (obj == null) return RedirectToAction(nameof(Error), new { Message = "Id not found" });

			return View(obj);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return RedirectToAction(nameof(Error), new { Message = "Id not provided" });

			var obj = await _sellerService.FindByIdAsync(id.Value);

			if (obj == null) return RedirectToAction(nameof(Error), new { Message = "Id not found" });

			List<Department> departments = await _departmentService.FindAllAsync();
			SellerFormViewModel viewModel = new SellerFormViewModel { Seller = obj, Departments = departments };
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Seller obj)
		{
			if (!ModelState.IsValid)
			{
				var departments = await _departmentService.FindAllAsync();
				var viewModel = new SellerFormViewModel { Seller = obj, Departments = departments };
				return View(viewModel);
			}
			
			if (id != obj.Id)
			{
				return RedirectToAction(nameof(Error), new { message = "Id mismatch" });
			}
			
			try
			{
				await _sellerService.UpdateAsync(obj);
				return RedirectToAction(nameof(Index));
			}
			catch (ApplicationException e)
			{
				return RedirectToAction(nameof(Error), new { message = e.Message });
			}
		}

		public IActionResult Error(string message)
		{
			var viewModel = new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
				Message = message
			};

			return View(viewModel);
		}
	}
}
