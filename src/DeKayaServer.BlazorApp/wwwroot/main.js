// Sidebar ve Menü JavaScript

(function () {
    'use strict';

    function getSidebar() {
        return document.getElementById('sidebar');
    }

    function getMainWrapper() {
        return document.querySelector('.main-wrapper');
    }

    function applyDesktopMargin(sidebar, mainWrapper) {
        if (!sidebar || !mainWrapper) return;

        // Desktop: collapsed/expanded margin
        if (sidebar.classList.contains('collapsed')) {
            mainWrapper.style.marginLeft = '70px';
        } else {
            mainWrapper.style.marginLeft = '280px';
        }
    }

    function applyResponsiveLayout(sidebar, mainWrapper) {
        if (!sidebar || !mainWrapper) return;

        // Tablet/Mobile behavior
        if (window.innerWidth <= 992) {
            sidebar.classList.remove('collapsed'); // force expanded on mobile/tablet
            mainWrapper.style.marginLeft = '0';
            return;
        }

        // Desktop behavior from localStorage
        const savedState = localStorage.getItem('sidebarCollapsed');
        if (savedState === 'true') {
            sidebar.classList.add('collapsed');
        } else {
            sidebar.classList.remove('collapsed');
        }

        applyDesktopMargin(sidebar, mainWrapper);
    }

    function toggleSidebarCollapsed() {
        const sidebar = getSidebar();
        const mainWrapper = getMainWrapper();
        if (!sidebar || !mainWrapper) return;

        sidebar.classList.toggle('collapsed');
        applyDesktopMargin(sidebar, mainWrapper);

        // Local storage'a kaydet
        localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
    }

    function setMobileSidebarVisible(isVisible) {
        const sidebar = getSidebar();
        if (!sidebar) return;

        sidebar.classList.toggle('show', isVisible);

        // Backdrop ekle/kaldır
        let backdrop = document.querySelector('.sidebar-backdrop');

        if (isVisible) {
            if (!backdrop) {
                backdrop = document.createElement('div');
                backdrop.className = 'sidebar-backdrop';
                document.body.appendChild(backdrop);

                // Backdrop'e tıklandığında sidebar'ı kapat
                backdrop.addEventListener('click', function () {
                    setMobileSidebarVisible(false);
                    backdrop.remove();
                });
            }
        } else {
            backdrop?.remove();
        }
    }

    function toggleMobileSidebar() {
        const sidebar = getSidebar();
        if (!sidebar) return;

        const isVisible = !sidebar.classList.contains('show');
        setMobileSidebarVisible(isVisible);
    }

    function wireMenuSubmenus() {
        // Submenu Toggle (static DOM binding; safe to call multiple times)
        const menuItems = document.querySelectorAll('.menu-item');
        menuItems.forEach(item => {
            const menuLink = item.querySelector('.menu-link');
            const submenu = item.querySelector('.submenu');
            if (!submenu || !menuLink) return;

            // prevent duplicate binding
            if (menuLink.dataset.submenuBound === '1') return;
            menuLink.dataset.submenuBound = '1';

            menuLink.addEventListener('click', function (e) {
                e.preventDefault();

                // Diğer açık menüleri kapat (accordion effect)
                menuItems.forEach(otherItem => {
                    if (otherItem !== item && otherItem.classList.contains('open')) {
                        otherItem.classList.remove('open');
                        const otherSubmenu = otherItem.querySelector('.submenu');
                        if (otherSubmenu) otherSubmenu.style.maxHeight = '0';
                    }
                });

                // Toggle current menu
                item.classList.toggle('open');

                // Smooth height animation
                if (item.classList.contains('open')) {
                    submenu.style.maxHeight = submenu.scrollHeight + 'px';
                } else {
                    submenu.style.maxHeight = '0';
                }
            });
        });
    }

    function highlightActiveMenu() {
        const currentPath = window.location.pathname;
        const menuLinks = document.querySelectorAll('.menu-link, .submenu a');

        menuLinks.forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                const menuItem = link.closest('.menu-item');
                if (!menuItem) return;

                menuItem.classList.add('active');

                // Open parent menu if in submenu
                const parentItem = link.closest('.submenu')?.closest('.menu-item');
                if (parentItem) {
                    parentItem.classList.add('open');
                    const parentSubmenu = parentItem.querySelector('.submenu');
                    if (parentSubmenu) {
                        parentSubmenu.style.maxHeight = parentSubmenu.scrollHeight + 'px';
                    }
                }
            }
        });
    }

    // DOM ready: initial setup
    document.addEventListener('DOMContentLoaded', function () {
        const sidebar = getSidebar();
        const mainWrapper = getMainWrapper();

        // Desktop sidebar toggle button (inside sidebar)
        const sidebarToggle = document.getElementById('sidebarToggle');
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function () {
                toggleSidebarCollapsed();
            });
        }

        wireMenuSubmenus();
        highlightActiveMenu();

        applyResponsiveLayout(sidebar, mainWrapper);

        // Window resize handler
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                applyResponsiveLayout(getSidebar(), getMainWrapper());

                // if switching to desktop, ensure mobile state is closed
                if (window.innerWidth > 992) {
                    setMobileSidebarVisible(false);
                }
            }, 250);
        });
    });

    // IMPORTANT: Blazor render'ında header/btn sonradan geldiği için delegation kullan
    document.addEventListener('click', function (e) {
        // Mobile top-left toggle
        const mobileToggle = e.target.closest?.('.mobile-sidebar-toggle');
        if (mobileToggle) {
            e.preventDefault();
            toggleMobileSidebar();
            return;
        }

        // If user clicks a nav link on mobile, close sidebar
        const submenuLink = e.target.closest?.('#sidebar .submenu a, #sidebar .menu-link');
        if (submenuLink && window.innerWidth <= 992) {
            setMobileSidebarVisible(false);
            return;
        }
    });

    // Export functions for use in other scripts
    window.sidebarUtils = {
        toggleSidebar: function () {
            document.getElementById('sidebarToggle')?.click();
        },
        // keep legacy name if used elsewhere
        toggleMobileSidebar: function () {
            toggleMobileSidebar();
        },
        showNotification: (typeof showNotification !== 'undefined') ? showNotification : undefined
    };
})();