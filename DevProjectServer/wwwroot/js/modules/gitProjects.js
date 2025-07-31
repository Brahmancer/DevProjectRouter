class GitProjectManager {
    constructor () {
        this.modalId = '#createProjectModal';
        this.initializeEventListeners();
        this.loadProjects(); // Load projects on page load
    }
    
    initializeEventListeners () {
        // Load modal content
        $(document).on('click', '[data-bs-target="#createProjectModal"]', () => {
            this.loadModal();
        });

        // Handle form submission
        $(document).on('submit', '#createProjectForm', (e) => {
            e.preventDefault();
            this.handleFormSubmission();
        });

        // GitHub connection
        $(document).on('click', '#connectGitHubBtn', () => {
            window.location.href = '/GitHub/Connect';
        });

        // Sync repositories
        $(document).on('click', '#syncRepositoriesBtn', () => {
            this.syncRepositories();
        });

        // Load README
        $(document).on('click', '.load-readme-btn', (e) => {
            const projectId = $(e.target).data('project-id');
            this.loadReadme(projectId);
        });
    }
    
    async loadModal () {
        if ($(this.modalId).length === 0) {
            try {
                const response = await $.get('/GitProject/Create');
                $('#modalContainer').html(response);
                $(this.modalId).modal('show');
            } catch (error) {
                console.error('Failed to load modal:', error);
            }
        }
    }

    async handleFormSubmission() {
        const formData = {
            Name: $('#Name').val(),
            RepoUrl: $('#RepoUrl').val(),
            Description: $('#Description').val()
        };

        try {
            const response = await $.ajax({
                url: '/GitProject/Create',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            if (response.success) {
                this.showAlert('success', response.message);
                $(this.modalId).modal('hide');
                $('#createProjectForm')[0].reset();
                this.loadProjects();
            } else {
                this.showAlert('danger', `Error: ${response.message}`);
            }
        } catch (error) {
            this.showAlert('danger', 'An error occurred while creating the project.');
        }
    }

    async syncRepositories() {
        try {
            const response = await $.ajax({
                url: '/GitHub/SyncRepositories',
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            if (response.success) {
                this.showAlert('success', response.message);
                this.loadProjects();
                $('#syncRepositoriesBtn').show();
            } else {
                this.showAlert('danger', response.message);
            }
        } catch (error) {
            this.showAlert('danger', 'An error occurred while syncing repositories.');
        }
    }

    async loadReadme(projectId) {
        try {
            const response = await $.get(`/GitHub/GetReadme?projectId=${projectId}`);
            
            if (response.success) {
                $(`#readme-${projectId}`).html(response.html);
                $(`#readme-${projectId}`).show();
                $(`.load-readme-btn[data-project-id="${projectId}"]`).hide();
            } else {
                this.showAlert('warning', response.message);
            }
        } catch (error) {
            this.showAlert('danger', 'An error occurred while loading README.');
        }
    }

    showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        $('.container').prepend(alertHtml);

        setTimeout(() => {
            $('.alert').alert('close');
        }, 5000);
    }

    async loadProjects() {
        try {
            const response = await $.get('/GitProject/GetUserProjectsHtml');
            $('#projectsContainer').html(response);
            
            // Show sync button if projects exist
            if ($('.project-card').length > 0) {
                $('#syncRepositoriesBtn').show();
            }
        } catch (error) {
            console.error('Failed to load projects');
            this.showAlert('danger', 'Error loading projects');
        }
    }    
}

// Initialize when document is ready
$(document).ready(() => {
    new GitProjectManager();
});