// ═══════════════════════════════════════════════════
//  CAR DEALERSHIP FRONTEND — API SERVICE & APP LOGIC
// ═══════════════════════════════════════════════════

const API = 'http://localhost:5221/api';

// ─── Auth helpers ───
function getToken() { return localStorage.getItem('token'); }
function getUser() { try { return JSON.parse(localStorage.getItem('user')); } catch { return null; } }
function isAdmin() { const u = getUser(); return u && u.role === 'Admin'; }

function saveAuth(data) {
  localStorage.setItem('token', data.token);
  localStorage.setItem('user', JSON.stringify({ name: data.name, role: data.role }));
}

function clearAuth() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
}

// ─── Fetch wrapper ───
async function api(endpoint, options = {}) {
  const headers = { 'Content-Type': 'application/json', ...options.headers };
  const token = getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${API}${endpoint}`, { ...options, headers });

  if (res.status === 401) {
    clearAuth();
    renderApp();
    throw new Error('Session expired. Please login again.');
  }

  let body;
  const ct = res.headers.get('content-type');
  if (ct && ct.includes('application/json')) {
    body = await res.json();
  } else {
    body = await res.text();
  }

  if (!res.ok) {
    const msg = typeof body === 'string' ? body : (body.title || body.message || JSON.stringify(body));
    throw new Error(msg);
  }
  return body;
}

// ─── Toast notifications ───
function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.textContent = message;
  container.appendChild(toast);
  setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 300); }, 3500);
}

// ─── Navigation ───
let currentPage = 'cars';

function navigateTo(page) {
  currentPage = page;
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));

  const pageEl = document.getElementById(`page-${page}`);
  const navEl = document.querySelector(`[data-page="${page}"]`);
  if (pageEl) pageEl.classList.add('active');
  if (navEl) navEl.classList.add('active');

  // Load data for the page
  switch (page) {
    case 'cars': loadCars(); break;
    case 'my-offers': loadMyOffers(); break;
    case 'all-offers': loadAllOffers(); break;
    case 'users': loadUsers(); break;
  }
}

// ═══════════════════════════════
//  AUTH: Register & Login
// ═══════════════════════════════

async function handleRegister(e) {
  e.preventDefault();
  const btn = e.target.querySelector('button[type="submit"]');
  btn.disabled = true;
  btn.textContent = 'Creating account...';
  try {
    const name = document.getElementById('reg-name').value;
    const email = document.getElementById('reg-email').value;
    const password = document.getElementById('reg-password').value;
    await api('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ name, email, password })
    });
    showToast('Registration successful! Please login.', 'success');
    showLogin();
  } catch (err) {
    showToast(err.message, 'error');
  } finally {
    btn.disabled = false;
    btn.textContent = 'Create Account';
  }
}

async function handleLogin(e) {
  e.preventDefault();
  const btn = e.target.querySelector('button[type="submit"]');
  btn.disabled = true;
  btn.textContent = 'Signing in...';
  try {
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    const data = await api('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
    saveAuth(data);
    showToast(`Welcome back, ${data.name}!`, 'success');
    renderApp();
  } catch (err) {
    showToast(err.message, 'error');
  } finally {
    btn.disabled = false;
    btn.textContent = 'Sign In';
  }
}

function showLogin() {
  document.getElementById('auth-login').style.display = 'block';
  document.getElementById('auth-register').style.display = 'none';
}

function showRegister() {
  document.getElementById('auth-login').style.display = 'none';
  document.getElementById('auth-register').style.display = 'block';
}

function logout() {
  clearAuth();
  showToast('Logged out successfully', 'info');
  renderApp();
}

// ═══════════════════════════════
//  CARS
// ═══════════════════════════════

async function loadCars() {
  const grid = document.getElementById('cars-grid');
  grid.innerHTML = '<div class="loading">Loading cars</div>';
  try {
    const cars = await api('/cars');
    if (cars.length === 0) {
      grid.innerHTML = '<div class="empty-state"><div class="empty-icon">🚗</div><p>No cars available right now.</p></div>';
      return;
    }
    grid.innerHTML = cars.map(car => `
      <div class="card car-card" style="animation: fadeInUp 0.4s ease ${Math.random() * 0.2}s both">
        <div class="car-header">
          <div>
            <div class="car-brand">${esc(car.brand)}</div>
            <div class="car-model">${esc(car.model)}</div>
          </div>
          <span class="car-year">${car.year}</span>
        </div>
        <div class="car-price">$${Number(car.price).toLocaleString()}</div>
        <span class="badge ${car.isAvailable ? 'badge-available' : 'badge-sold'}" style="margin-bottom:12px">
          ${car.isAvailable ? '● Available' : '● Sold'}
        </span>
        <div class="car-actions">
          ${getToken() && !isAdmin() && car.isAvailable ? `<button class="btn btn-primary btn-sm" onclick="openOfferModal(${car.id}, '${esc(car.brand)} ${esc(car.model)}', ${car.price})">💰 Make Offer</button>` : ''}
          ${isAdmin() ? `
            <button class="btn btn-ghost btn-sm" onclick="openEditCarModal(${car.id}, '${esc(car.brand)}', '${esc(car.model)}', ${car.price}, ${car.year})">✏️ Edit</button>
            <button class="btn btn-danger btn-sm" onclick="deleteCar(${car.id})">🗑 Delete</button>
          ` : ''}
        </div>
      </div>
    `).join('');
  } catch (err) {
    grid.innerHTML = `<div class="empty-state"><div class="empty-icon">⚠️</div><p>${esc(err.message)}</p></div>`;
  }
}

async function handleAddCar(e) {
  e.preventDefault();
  try {
    const car = {
      brand: document.getElementById('car-brand').value,
      model: document.getElementById('car-model').value,
      price: parseFloat(document.getElementById('car-price').value),
      year: parseInt(document.getElementById('car-year').value)
    };
    await api('/cars', { method: 'POST', body: JSON.stringify(car) });
    showToast('Car added successfully!', 'success');
    closeModal('modal-add-car');
    loadCars();
  } catch (err) { showToast(err.message, 'error'); }
}

function openEditCarModal(id, brand, model, price, year) {
  document.getElementById('edit-car-id').value = id;
  document.getElementById('edit-car-brand').value = brand;
  document.getElementById('edit-car-model').value = model;
  document.getElementById('edit-car-price').value = price;
  document.getElementById('edit-car-year').value = year;
  openModal('modal-edit-car');
}

async function handleEditCar(e) {
  e.preventDefault();
  const id = document.getElementById('edit-car-id').value;
  try {
    const car = {
      brand: document.getElementById('edit-car-brand').value,
      model: document.getElementById('edit-car-model').value,
      price: parseFloat(document.getElementById('edit-car-price').value),
      year: parseInt(document.getElementById('edit-car-year').value)
    };
    await api(`/cars/${id}`, { method: 'PUT', body: JSON.stringify(car) });
    showToast('Car updated!', 'success');
    closeModal('modal-edit-car');
    loadCars();
  } catch (err) { showToast(err.message, 'error'); }
}

async function deleteCar(id) {
  if (!confirm('Delete this car?')) return;
  try {
    await api(`/cars/${id}`, { method: 'DELETE' });
    showToast('Car deleted', 'success');
    loadCars();
  } catch (err) { showToast(err.message, 'error'); }
}

// ═══════════════════════════════
//  OFFERS
// ═══════════════════════════════

function openOfferModal(carId, carName, askingPrice) {
  document.getElementById('offer-car-id').value = carId;
  document.getElementById('offer-car-name').textContent = carName;
  document.getElementById('offer-asking-price').textContent = `$${Number(askingPrice).toLocaleString()}`;
  document.getElementById('offer-amount').value = '';
  openModal('modal-make-offer');
}

async function handleMakeOffer(e) {
  e.preventDefault();
  try {
    const offer = {
      carId: parseInt(document.getElementById('offer-car-id').value),
      amount: parseFloat(document.getElementById('offer-amount').value)
    };
    await api('/offers', { method: 'POST', body: JSON.stringify(offer) });
    showToast('Offer submitted!', 'success');
    closeModal('modal-make-offer');
  } catch (err) { showToast(err.message, 'error'); }
}

async function loadMyOffers() {
  const container = document.getElementById('my-offers-list');
  container.innerHTML = '<div class="loading">Loading your offers</div>';
  try {
    const offers = await api('/offers/my');
    if (offers.length === 0) {
      container.innerHTML = '<div class="empty-state"><div class="empty-icon">📋</div><p>You haven\'t made any offers yet.</p></div>';
      return;
    }
    container.innerHTML = `<div class="card"><table class="data-table">
      <thead><tr><th>Car</th><th>Your Offer</th><th>Status</th></tr></thead>
      <tbody>${offers.map(o => `<tr>
        <td><strong>${esc(o.car?.brand || '')} ${esc(o.car?.model || '')}</strong></td>
        <td>$${Number(o.amount).toLocaleString()}</td>
        <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
      </tr>`).join('')}</tbody>
    </table></div>`;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-icon">⚠️</div><p>${esc(err.message)}</p></div>`;
  }
}

async function loadAllOffers() {
  const container = document.getElementById('all-offers-list');
  container.innerHTML = '<div class="loading">Loading all offers</div>';
  try {
    const offers = await api('/offers');
    if (offers.length === 0) {
      container.innerHTML = '<div class="empty-state"><div class="empty-icon">📋</div><p>No offers yet.</p></div>';
      return;
    }
    // Stats
    const pending = offers.filter(o => o.status === 'Pending').length;
    const accepted = offers.filter(o => o.status === 'Accepted').length;
    const rejected = offers.filter(o => o.status === 'Rejected').length;

    container.innerHTML = `
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-label">Pending</div><div class="stat-value warning">${pending}</div></div>
        <div class="stat-card"><div class="stat-label">Accepted</div><div class="stat-value success">${accepted}</div></div>
        <div class="stat-card"><div class="stat-label">Rejected</div><div class="stat-value" style="color:var(--danger)">${rejected}</div></div>
      </div>
      <div class="card"><table class="data-table">
        <thead><tr><th>Customer</th><th>Car</th><th>Amount</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>${offers.map(o => `<tr>
          <td>${esc(o.user?.name || 'N/A')}</td>
          <td><strong>${esc(o.car?.brand || '')} ${esc(o.car?.model || '')}</strong></td>
          <td>$${Number(o.amount).toLocaleString()}</td>
          <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
          <td>${o.status === 'Pending' ? `
            <button class="btn btn-success btn-sm" onclick="updateOfferStatus(${o.id}, 'Accepted')">✓ Accept</button>
            <button class="btn btn-danger btn-sm" onclick="updateOfferStatus(${o.id}, 'Rejected')">✕ Reject</button>
          ` : '—'}</td>
        </tr>`).join('')}</tbody>
      </table></div>`;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-icon">⚠️</div><p>${esc(err.message)}</p></div>`;
  }
}

async function updateOfferStatus(id, status) {
  try {
    await api(`/offers/${id}/status`, { method: 'PUT', body: JSON.stringify({ status }) });
    showToast(`Offer ${status.toLowerCase()}!`, status === 'Accepted' ? 'success' : 'info');
    loadAllOffers();
  } catch (err) { showToast(err.message, 'error'); }
}

// ═══════════════════════════════
//  USERS (Admin)
// ═══════════════════════════════

async function loadUsers() {
  const container = document.getElementById('users-list');
  container.innerHTML = '<div class="loading">Loading users</div>';
  try {
    const users = await api('/users');
    if (users.length === 0) {
      container.innerHTML = '<div class="empty-state"><div class="empty-icon">👤</div><p>No users found.</p></div>';
      return;
    }
    container.innerHTML = `<div class="card"><table class="data-table">
      <thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th>Actions</th></tr></thead>
      <tbody>${users.map(u => `<tr>
        <td>#${u.id}</td>
        <td><strong>${esc(u.name)}</strong></td>
        <td>${esc(u.email)}</td>
        <td><span class="badge badge-${u.role.toLowerCase()}">${u.role}</span></td>
        <td>${u.isBanned ? '<span class="badge badge-banned">Banned</span>' : '<span class="badge badge-available">Active</span>'}</td>
        <td>${u.role !== 'Admin' ? (u.isBanned
          ? `<button class="btn btn-success btn-sm" onclick="unbanUser(${u.id})">Unban</button>`
          : `<button class="btn btn-warning btn-sm" onclick="banUser(${u.id})">Ban</button>`
        ) : '—'}</td>
      </tr>`).join('')}</tbody>
    </table></div>`;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-icon">⚠️</div><p>${esc(err.message)}</p></div>`;
  }
}

async function banUser(id) {
  if (!confirm('Ban this user?')) return;
  try {
    const msg = await api(`/users/${id}/ban`, { method: 'PUT' });
    showToast(msg, 'success');
    loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

async function unbanUser(id) {
  try {
    const msg = await api(`/users/${id}/unban`, { method: 'PUT' });
    showToast(msg, 'success');
    loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

// ═══════════════════════════════
//  MODAL HELPERS
// ═══════════════════════════════

function openModal(id) { document.getElementById(id).classList.add('active'); }
function closeModal(id) { document.getElementById(id).classList.remove('active'); }

// Close modal on backdrop click
document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('active');
  }
});

// ─── XSS helper ───
function esc(str) {
  const d = document.createElement('div');
  d.textContent = str;
  return d.innerHTML;
}

// ═══════════════════════════════
//  RENDER APP (auth vs dashboard)
// ═══════════════════════════════

function renderApp() {
  const token = getToken();
  const user = getUser();

  if (!token) {
    document.getElementById('auth-view').style.display = 'flex';
    document.getElementById('app-view').style.display = 'none';
    showLogin();
    return;
  }

  document.getElementById('auth-view').style.display = 'none';
  document.getElementById('app-view').style.display = 'flex';

  // Update user info in sidebar
  const nameEl = document.getElementById('sidebar-user-name');
  const roleEl = document.getElementById('sidebar-user-role');
  const avatarEl = document.getElementById('sidebar-user-avatar');
  if (user) {
    nameEl.textContent = user.name;
    roleEl.textContent = user.role;
    avatarEl.textContent = user.name.charAt(0).toUpperCase();
  }

  // Show/hide admin nav links
  document.querySelectorAll('.admin-only').forEach(el => {
    el.style.display = isAdmin() ? 'flex' : 'none';
  });
  document.querySelectorAll('.customer-only').forEach(el => {
    el.style.display = !isAdmin() ? 'flex' : 'none';
  });

  navigateTo('cars');
}

// ─── Init ───
document.addEventListener('DOMContentLoaded', renderApp);
