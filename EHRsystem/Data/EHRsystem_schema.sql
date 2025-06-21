-- Create the Database
CREATE DATABASE IF NOT EXISTS ehr_system;
USE ehr_system;

-- Patient Table
CREATE TABLE Patient (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(100) UNIQUE,
    password VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    date_of_birth DATE,
    gender ENUM('Male', 'Female', 'Other')
);

-- Doctor Table
CREATE TABLE Doctor (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(100) UNIQUE,
    password VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    specialization VARCHAR(100),
    address VARCHAR(255)
);

-- Admin Table
CREATE TABLE Admin (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(100) UNIQUE,
    password VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100)
);

-- DoctorLocations Table
CREATE TABLE DoctorLocations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    doctor_id INT NOT NULL,
    location_name VARCHAR(100),
    address VARCHAR(255),
    FOREIGN KEY (doctor_id) REFERENCES Doctor(id)
);

-- MedicalHistory Table (one-to-one with Patient)
CREATE TABLE MedicalHistory (
    id INT AUTO_INCREMENT PRIMARY KEY,
    patient_id INT NOT NULL UNIQUE,
    past_illnesses JSON,
    family_history JSON,
    lifestyle_factors JSON,
    chronic_conditions JSON,
    mental_health_history JSON,
    FOREIGN KEY (patient_id) REFERENCES Patient(id)
);

-- Surgeries Table
CREATE TABLE Surgeries (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medical_history_id INT NOT NULL,
    surgery_type VARCHAR(100),
    surgery_date DATE,
    FOREIGN KEY (medical_history_id) REFERENCES MedicalHistory(id)
);

-- Medications Table
CREATE TABLE Medications (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medical_history_id INT NOT NULL,
    medication_name VARCHAR(100),
    status ENUM('current', 'past'),
    FOREIGN KEY (medical_history_id) REFERENCES MedicalHistory(id)
);

-- Allergies Table
CREATE TABLE Allergies (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medical_history_id INT NOT NULL,
    allergen VARCHAR(100),
    reaction VARCHAR(255),
    FOREIGN KEY (medical_history_id) REFERENCES MedicalHistory(id)
);

-- Vaccinations Table
CREATE TABLE Vaccinations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medical_history_id INT NOT NULL,
    vaccine_name VARCHAR(100),
    vaccination_date DATE,
    FOREIGN KEY (medical_history_id) REFERENCES MedicalHistory(id)
);

-- Hospitalizations Table
CREATE TABLE Hospitalizations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medical_history_id INT NOT NULL,
    reason TEXT,
    hospitalization_date DATE,
    FOREIGN KEY (medical_history_id) REFERENCES MedicalHistory(id)
);

-- Appointments Table
CREATE TABLE Appointments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    patient_id INT NOT NULL,
    doctor_id INT NOT NULL,
    appointment_datetime DATETIME NOT NULL,
    status ENUM('scheduled', 'completed', 'cancelled'),
    reason TEXT,
    reminder_sent BOOLEAN DEFAULT FALSE,
    consultation_type ENUM('in-person', 'online') DEFAULT 'in-person',
    meeting_link VARCHAR(255) DEFAULT NULL,
    FOREIGN KEY (patient_id) REFERENCES Patient(id),
    FOREIGN KEY (doctor_id) REFERENCES Doctor(id)
);

-- Prescription Table
CREATE TABLE Prescription (
    id INT AUTO_INCREMENT PRIMARY KEY,
    patient_id INT NOT NULL,
    doctor_id INT NOT NULL,
    prescription_date DATE NOT NULL,
    medication_name VARCHAR(100),
    dosage VARCHAR(50),
    duration VARCHAR(50),
    notes TEXT,
    FOREIGN KEY (patient_id) REFERENCES Patient(id),
    FOREIGN KEY (doctor_id) REFERENCES Doctor(id)
);

-- MedicalLabs Table
CREATE TABLE MedicalLabs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    patient_id INT NOT NULL,
    doctor_id INT NOT NULL,
    lab_date DATE NOT NULL,
    test_type VARCHAR(100),
    results TEXT,
    status ENUM('pending', 'completed'),
    FOREIGN KEY (patient_id) REFERENCES Patient(id),
    FOREIGN KEY (doctor_id) REFERENCES Doctor(id)
);

-- Messages Table (chat system)
CREATE TABLE Messages (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sender_id INT NOT NULL,
    receiver_id INT NOT NULL,
    message_content TEXT NOT NULL,
    sent_datetime DATETIME NOT NULL,
    FOREIGN KEY (sender_id) REFERENCES Patient(id) ON DELETE CASCADE,
    FOREIGN KEY (receiver_id) REFERENCES Doctor(id) ON DELETE CASCADE
);

-- PharmacyRecommendations Table
CREATE TABLE PharmacyRecommendations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    prescription_id INT NOT NULL,
    pharmacy_name VARCHAR(100) NOT NULL,
    purchase_link VARCHAR(255) NOT NULL,
    recommended_date DATE NOT NULL,
    FOREIGN KEY (prescription_id) REFERENCES Prescription(id) ON DELETE CASCADE
);
