let currentPage = 1;
let isLoading = false;
let hasMoreBooks = true;
const memberId = document.getElementById("MemberId").dataset.memberId;

// DOM Elements
const booksContainer = document.getElementById("booksContainer");
const loader = document.getElementById("loader");
const sentinel = document.getElementById("scrollSentinel");

// === Helper UI Functions ===
function showLoader() {
	loader.style.display = "block";
}

function hideLoader() {
	loader.style.display = "none";
}

function appendBooks(html) {
	booksContainer.insertAdjacentHTML("beforeend", html);
}

function removeSentinel() {
	if (sentinel) {
		sentinel.remove();
	}
}

function handleError(error) {
	console.error("Error loading books:", error);
	showErrorToast("Error loading books. Please try again later.");
}

// === Main Data Loading Function ===

async function loadBooks() {
	if (isLoading || !hasMoreBooks) return;

	isLoading = true;
	showLoader();

	try {
		const response = await fetch(`/Community/Members/GetAllMemberBooks?memberId=${memberId}&page=${currentPage}`);

		if (!response.ok) {
			const errorMessage = await response.text();
			throw new Error(errorMessage || "Failed to fetch books.");
		}

		const html = await response.text();

		// لو مفيش كتب جاية من السيرفر
		if (html.trim() === "") {
			hasMoreBooks = false;
			removeSentinel();
		} else {
			appendBooks(html);
			currentPage++;
		}
	} catch (error) {
		handleError(error);
	} finally {
		isLoading = false;
		hideLoader();
	}
}

// === Intersection Observer Setup ===

const observer = new IntersectionObserver(entries => {
	if (entries[0].isIntersecting) { // Check if the sentinel is in view (visible)
		loadBooks();
	}
}, {
	root: null, // Use the viewport as the root (null means viewport)
	rootMargin: "0px", // No margin around the root
	threshold: 1.0 // Trigger when 100% of the sentinel is visible
});

observer.observe(sentinel);

// === Initial Load ===
document.addEventListener("DOMContentLoaded", () => {
	AOS.init({ duration: 600, easing: "ease-in-out" });
	loadBooks();
});