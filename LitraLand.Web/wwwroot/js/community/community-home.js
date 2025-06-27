// community-home.js

// --- App State ---
let currentPage = 1;
const pageSize = 10;
let isLoading = false;
let hasMore = true;

// --- DOM References ---
const DOM = {
    booksContainer: document.getElementById("booksContainer"),
    loader: document.getElementById("loader"),
    noBooksMessage: document.getElementById("noBooksMessage"),
    searchInput: document.getElementById("booksSearchInput"),
    searchButton: document.getElementById("booksSearchBtn"),
    filterSelect: document.getElementById("FilterType"),
    scrollSentinel: document.getElementById("scrollSentinel"),
    antiForgeryToken: document.querySelector("input[name='__RequestVerificationToken']").value
};

// --- Utility Functions ---
function debounce(func, delay) {
    let timeout;
    return (...args) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), delay);
    };
}

function resetAndLoad() {
    currentPage = 1;
    hasMore = true;
    loadBooks();
}

function handleLoadMore() {
    if (!isLoading && hasMore) {
        loadBooks();
    }
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function handleError(error) {
    showErrorToast(error.message || "An error occurred while loading books.");
}

// --- Render Book Card ---
function renderBookCard(book) {
    const truncate = (str, maxLength) =>
        str.length > maxLength ? str.substring(0, maxLength) + "..." : str;

    const truncatedUserName = truncate(book.ownerUserName, 10);
    const truncatedBookTitle = truncate(book.title, 16);
    const truncatedAuthorName = truncate(book.author, 12);

    return `
        <div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-3 community-book-container" data-aos="fade-up">
            <div class="m-0">
                <div class="card-rounded overflow-hidden position-relative w-100 mb-3" style="aspect-ratio: 3/4;">
                    <div class="bgi-position-center bgi-no-repeat bgi-size-cover w-100 h-100" style="background-image:url('${book.imageThumbnailUrl}');"></div>
                </div>
                <div class="m-0">
                    <a href="/Community/CommunityBooks/Details?key=${book.key}">
                        <!-- Title (Truncated for md+) -->
                        <h5 class="text-gray-800 text-hover-primary fs-5 fw-bold d-none d-md-block mb-1" title="${book.title}">
                            ${truncatedBookTitle}
                        </h5>
                        <!-- Title (Full for small screens) -->
                        <h5 class="text-gray-800 text-hover-primary fs-5 fw-bold d-block d-md-none mb-1">
                            ${book.title}
                        </h5>
                    </a>

                    <!-- Author -->
                    <span class="fw-bold fs-6 text-gray-500 d-none d-md-block mb-1" title="${book.author}">
                        ${truncatedAuthorName}
                    </span>
                    <span class="fw-bold fs-6 text-gray-500 d-block d-md-none mb-1">
                        ${book.author}
                    </span>

                    <!-- Owner -->
                    <div class="text-gray-600 small mb-1">
                        <i class="bi bi-person-circle me-1 text-primary"></i>
                        Owner:
                        <a href="/Community/Members/Profile/${book.ownerId}" class="text-decoration-none">
                            <strong class="d-none d-md-inline" title="${book.ownerUserName}">${truncatedUserName}</strong>
                            <strong class="d-inline d-md-none">${book.ownerUserName}</strong>
                        </a>
                    </div>

                    <!-- Badges -->
                    <div class="text-gray-600 small mb-1 d-flex flex-wrap gap-2 align-items-center">
                        <div class="d-flex align-items-center">
                            <i class="bi bi-arrow-left-right me-1 ${book.isForExchange ? "text-success" : "text-muted"}"></i>
                            <span class="badge ${book.isForExchange ? "badge-light-success" : "bg-light text-muted border"}"
                                  title="${book.isForExchange ? "This book is available for exchange" : "This book is not for exchange"}">
                                ${book.isForExchange ? "For Exchange" : "Not For Exchange"}
                            </span>
                        </div>
                    </div>

                    <!-- Price -->
                    <div class="text-gray-600 small mb-1">
                        <i class="bi bi-cash-coin me-1 text-success"></i>
                        Price: <strong>${book.price.toFixed(2)} EGP</strong>
                    </div>
                </div>
            </div>
        </div>
    `;
}

function renderBooks(books) {
    books.forEach(book => {
        DOM.booksContainer.insertAdjacentHTML("beforeend", renderBookCard(book));
    });
    AOS.refresh();
}

// --- Load Books ---
async function loadBooks() {
    if (isLoading || !hasMore) return;

    isLoading = true;
    DOM.loader.style.display = "block";

    const searchTerm = DOM.searchInput.value.trim();
    const filter = DOM.filterSelect.value;

    const formData = new FormData();
    formData.append("page", currentPage);
    formData.append("pageSize", pageSize);
    formData.append("searchTerm", searchTerm);
    formData.append("filter", filter);
    formData.append("__RequestVerificationToken", DOM.antiForgeryToken);

    try {
        const response = await fetch("/Community/CommunityHome/LoadBooks", {
            method: "POST",
            body: formData
        });

        if (!response.ok) throw new Error("Failed to load books");

        const result = await response.json();
        DOM.loader.style.display = "none";

        if (currentPage === 1) {
            DOM.booksContainer.innerHTML = "";
            scrollToTop();
        }

        if (result.books.length === 0 && currentPage === 1) {
            DOM.noBooksMessage.classList.remove("d-none");
        } else {
            DOM.noBooksMessage.classList.add("d-none");
            renderBooks(result.books);

            if (!result.hasMore) {
                DOM.booksContainer.insertAdjacentHTML("beforeend", `
                    <div class="text-center text-muted mt-4 mb-2" data-aos="fade-up">You've reached the end of the list.</div>
                `);
            }
        }

        hasMore = result.hasMore;
        if (hasMore) currentPage++;

    } catch (error) {
        handleError(error);
        if (currentPage === 1)
            DOM.noBooksMessage.classList.remove("d-none");
    } finally {
        isLoading = false;
        DOM.loader.style.display = "none";
    }
}

// --- Init Events ---
function initEventListeners() {
    DOM.searchButton.addEventListener("click", () => {
        const searchTerm = DOM.searchInput.value.trim();
        if (searchTerm === "") return; // لو مفيش حاجة في السيرش متعملش حاجة

        resetAndLoad();
    });

    DOM.filterSelect.addEventListener("change", resetAndLoad);

    DOM.searchInput.addEventListener("keypress", (e) => {
        if (e.key === "Enter") resetAndLoad();
    });

    DOM.searchInput.addEventListener("input", debounce(resetAndLoad, 500));

    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
            handleLoadMore();
        }
    }, {
        rootMargin: "200px"
    });

    observer.observe(DOM.scrollSentinel);
}

// --- Init App ---
document.addEventListener("DOMContentLoaded", () => {
    AOS.init({ duration: 600, easing: "ease-in-out" });
    loadBooks();
    initEventListeners();
});