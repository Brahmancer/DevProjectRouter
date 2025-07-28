class GitProjectManager {
    constructor () {
        this.modalId = '#createProjectModal';
        this.initializeEventListeners();
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

    loadProjects() {
        // TODO: Implement project loading
        console.log('Loading projects...');
    }
    
}

// Initialize when document is ready
$(document).ready(() => {
    new GitProjectManager();
});